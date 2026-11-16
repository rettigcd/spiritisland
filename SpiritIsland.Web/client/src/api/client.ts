import createClient from 'openapi-fetch';
import type { paths } from './types';
import type { GameSummary, NewGameRequest, OracleResponse, SetupOptions, StaticView } from './model';

const http = createClient<paths>({ baseUrl: '' });

/** Pulls the server's { error } message out of a failed response, whatever shape came back. */
function messageOf(error: unknown, fallback: string): string {
  const text = (error as { error?: unknown } | undefined)?.error;
  return typeof text === 'string' && text.length > 0 ? text : fallback;
}

/** Runs a call, turning a network failure into a message instead of an exception. */
async function attempt<T>(call: () => Promise<T>): Promise<T | { networkError: string }> {
  try {
    return await call();
  } catch (e) {
    return { networkError: e instanceof Error ? e.message : String(e) };
  }
}

export type Loaded<T> = { ok: true; value: T } | { ok: false; message: string };

export async function getSetupOptions(): Promise<Loaded<SetupOptions>> {
  const r = await attempt(() => http.GET('/api/options'));
  if ('networkError' in r) return { ok: false, message: r.networkError };
  return r.data ? { ok: true, value: r.data } : { ok: false, message: messageOf(r.error, 'Could not load the setup options.') };
}

export async function listGames(): Promise<Loaded<GameSummary[]>> {
  const r = await attempt(() => http.GET('/api/games'));
  if ('networkError' in r) return { ok: false, message: r.networkError };
  return r.data ? { ok: true, value: r.data } : { ok: false, message: messageOf(r.error, 'Could not list the games.') };
}

export async function createGame(request: NewGameRequest): Promise<Loaded<OracleResponse>> {
  const r = await attempt(() => http.POST('/api/games', { body: request }));
  if ('networkError' in r) return { ok: false, message: r.networkError };
  return r.data ? { ok: true, value: r.data } : { ok: false, message: messageOf(r.error, 'Could not start the game.') };
}

export async function getStaticView(gameId: string): Promise<Loaded<StaticView>> {
  const r = await attempt(() => http.GET('/api/games/{id}/static', { params: { path: { id: gameId } } }));
  if ('networkError' in r) return { ok: false, message: r.networkError };
  return r.data ? { ok: true, value: r.data } : { ok: false, message: messageOf(r.error, 'Could not load the board.') };
}

/**
 * What a decide call came to.
 *  - ok: the picks were accepted; response is where the game is now.
 *  - stale: the checkpoint the client built on is out of date (another tab moved the game on); response is where the
 *    game really is, with no picks pending.
 *  - rejected: a pick is not one of the options (400) or the game is gone (404).
 *  - failed: the server or the network failed (500, no connection).
 */
export type DecideResult =
  | { kind: 'ok'; response: OracleResponse }
  | { kind: 'stale'; response: OracleResponse }
  | { kind: 'rejected'; message: string }
  | { kind: 'failed'; message: string };

export async function decide(gameId: string, checkpointVersion: number, picks: string[]): Promise<DecideResult> {
  const r = await attempt(() => http.POST('/api/games/{id}/decide', {
    params: { path: { id: gameId } },
    body: { checkpointVersion, picks },
  }));
  if ('networkError' in r) return { kind: 'failed', message: r.networkError };

  const status = r.response.status;
  if (r.data) return { kind: 'ok', response: r.data };
  if (status === 409) return { kind: 'stale', response: r.error as OracleResponse };
  const message = messageOf(r.error, r.response.statusText || `Request failed (${status}).`);
  return status === 400 || status === 404 ? { kind: 'rejected', message } : { kind: 'failed', message };
}
