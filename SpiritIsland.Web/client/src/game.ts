import { useCallback, useRef, useState } from 'preact/hooks';
import { createGame, decide, getStaticView } from './api/client';
import type { NewGameRequest, OracleResponse, StaticView } from './api/model';

/**
 * Client state (docs/WebClient-Plan.md): the game id, the checkpoint version, the picks made so far in the action in
 * progress, the cached static view and the last response. Everything else comes from the server. Nothing here is kept
 * on the server between requests, so the picks live in localStorage: a refresh loses at most the current action.
 */
export interface GameState {
  gameId: string | null;
  checkpointVersion: number;
  picks: string[];
  response: OracleResponse | null;
  staticView: StaticView | null;
  busy: boolean;
  /** Something the player should know that is not an error, e.g. "the game moved on". */
  notice: string | null;
  error: string | null;
}

const empty: GameState = {
  gameId: null, checkpointVersion: 0, picks: [], response: null, staticView: null, busy: false, notice: null, error: null,
};

// ---- localStorage (every access guarded: it can be missing, full or blocked) ----

const CURRENT = 'si:current';
const saveKey = (id: string) => `si:game:${id}`;

interface Saved { checkpointVersion: number; picks: string[] }

function readStorage(key: string): string | null {
  try { return localStorage.getItem(key); } catch { return null; }
}
function writeStorage(key: string, value: string | null) {
  try { if (value === null) localStorage.removeItem(key); else localStorage.setItem(key, value); } catch { /* not saved: a refresh rolls back to the start of the action */ }
}

function loadSaved(id: string): Saved | null {
  const text = readStorage(saveKey(id));
  if (!text) return null;
  try {
    const s = JSON.parse(text) as Saved;
    return typeof s.checkpointVersion === 'number' && Array.isArray(s.picks) ? s : null;
  } catch { return null; }
}

export function currentGameId(): string | null { return readStorage(CURRENT); }

// ---- the hook ----

export interface GameActions {
  start(request: NewGameRequest): Promise<void>;
  /** Opens a saved game, continuing the action that was in progress when the page was last open. */
  open(gameId: string): Promise<void>;
  choose(optionId: string): Promise<void>;
  /** Drops the last pick of the action in progress and asks the server again. */
  undo(): Promise<void>;
  /** Back to the start screen. The game stays saved on the server. */
  leave(): void;
  dismissMessages(): void;
}

export function useGame(): [GameState, GameActions] {
  const [state, setState] = useState<GameState>(empty);
  const latest = useRef(state); // the current state, for use inside async calls
  latest.current = state;

  const update = (patch: Partial<GameState>) => {
    latest.current = { ...latest.current, ...patch };
    setState(latest.current);
  };

  /** Takes a response from the server as the new truth: replaces the picks, saves them and refreshes the static view. */
  const apply = useCallback(async (response: OracleResponse, notice: string | null = null) => {
    update({
      gameId: response.gameId, checkpointVersion: response.checkpointVersion, picks: response.picks,
      response, notice, error: null, busy: false,
    });
    writeStorage(CURRENT, response.gameId);
    writeStorage(saveKey(response.gameId), JSON.stringify({ checkpointVersion: response.checkpointVersion, picks: response.picks } satisfies Saved));

    if (latest.current.staticView?.staticVersion !== response.staticVersion) {
      const loaded = await getStaticView(response.gameId);
      if (loaded.ok) update({ staticView: loaded.value });
      else update({ error: loaded.message });
    }
  }, []);

  /** Asks the server about `picks`, built on `checkpointVersion`. Returns false when the server rejected them. */
  const ask = useCallback(async (gameId: string, checkpointVersion: number, picks: string[]): Promise<boolean> => {
    update({ busy: true, error: null });
    const result = await decide(gameId, checkpointVersion, picks);
    switch (result.kind) {
      case 'ok':
        await apply(result.response);
        return true;
      case 'stale':
        // Another tab (or a stale save) moved the game on: take the server's state and drop our picks.
        await apply(result.response, 'The game had moved on, so your unfinished action was discarded.');
        return false;
      case 'rejected':
      case 'failed':
        update({ busy: false, error: result.message });
        return false;
    }
  }, [apply]);

  const actions: GameActions = {
    async start(request) {
      update({ ...empty, busy: true });
      const created = await createGame(request);
      if (created.ok) await apply(created.value);
      else update({ busy: false, error: created.message });
    },

    async open(gameId) {
      update({ ...empty, gameId, busy: true });
      const saved = loadSaved(gameId);
      // Without a save, -1 is never the right version: the server answers "stale" with where the game really is.
      const ok = await ask(gameId, saved?.checkpointVersion ?? -1, saved?.picks ?? []);
      if (!saved) update({ notice: null }); // nothing was discarded: there was nothing saved
      if (!ok && latest.current.response === null && latest.current.error !== null) {
        // The saved picks no longer fit (or the game is gone). Ask again with none, to at least show the game.
        const error = latest.current.error;
        await ask(gameId, -1, []);
        if (latest.current.response !== null) update({ notice: `Your saved picks could not be replayed (${error}), so the action was restarted.`, error: null });
      }
    },

    async choose(optionId) {
      const s = latest.current;
      if (s.busy || !s.gameId) return;
      await ask(s.gameId, s.checkpointVersion, [...s.picks, optionId]); // a rejected pick changes nothing; the error shows
    },

    async undo() {
      const s = latest.current;
      if (s.busy || !s.gameId || s.picks.length === 0) return;
      await ask(s.gameId, s.checkpointVersion, s.picks.slice(0, -1));
    },

    leave() {
      writeStorage(CURRENT, null);
      update({ ...empty });
    },

    dismissMessages() { update({ notice: null, error: null }); },
  };

  return [state, actions];
}
