import { spiritImageUrl } from './spiritImage';
import { useEffect, useState } from 'preact/hooks';
import { getSetupOptions, listGames } from './api/client';
import type { GameSummary, NewGameRequest, SetupOptions } from './api/model';
import { gamesPlayedHere, noteSpirit, recentSpirits } from './recent';

interface Props {
  busy: boolean;
  error: string | null;
  onStart(request: NewGameRequest): void;
  onOpen(gameId: string): void;
}

/** The start screen: pick a spirit, board, adversary and seed; or go back to a saved game. */
export function Lobby({ busy, error, onStart, onOpen }: Props) {
  const [options, setOptions] = useState<SetupOptions | null>(null);
  const [games, setGames] = useState<GameSummary[]>([]);
  const [loadError, setLoadError] = useState<string | null>(null);

  const [recent, setRecent] = useState<string[]>(recentSpirits);
  const [spirit, setSpirit] = useState('');
  const [board, setBoard] = useState('');
  const [adversary, setAdversary] = useState('');
  const [level, setLevel] = useState(0);
  const [seed, setSeed] = useState('');

  useEffect(() => {
    getSetupOptions().then(r => {
      if (!r.ok) { setLoadError(r.message); return; }
      setOptions(r.value);
    });
    listGames().then(r => {
      if (!r.ok) return;
      const mine = gamesPlayedHere(r.value);
      setGames(mine);
      // Before any spirit has been noted, the spirits of the games played here are the recent ones.
      setRecent(now => now.length > 0 ? now : [...new Set(mine.map(g => g.spirit))].slice(0, 4));
    });
  }, []);

  const levels = options?.adversaries[adversary] ?? [];
  const spiritChoices = [...recent.filter(s => options?.spirits.includes(s)), ...(options?.spirits ?? []).filter(s => !recent.includes(s))];

  const submit = (e: Event) => {
    e.preventDefault();
    const parsed = seed.trim() === '' ? undefined : Number(seed);
    // No board chosen means any board: pick one now, so the game says which it is.
    noteSpirit(spirit);
    const boards = options?.boards ?? [];
    const chosenBoard = board || boards[Math.floor(Math.random() * boards.length)] || '';
    onStart({
      spirit, board: chosenBoard,
      seed: parsed !== undefined && Number.isInteger(parsed) ? parsed : undefined,
      adversary: adversary || undefined,
      adversaryLevel: adversary ? level : 0,
    });
  };

  return (
    <main class="lobby">
      <h1>Spirit Island</h1>
      {(error ?? loadError) && <p class="error" role="alert">{error ?? loadError}</p>}

      <form class="panel" onSubmit={submit}>
        <h2>New game</h2>
        <label>Spirit
          <select value={spirit} required onChange={e => setSpirit(e.currentTarget.value)}>
            <option value="" disabled>(choose a spirit)</option>
            {/* The spirits played last come first, marked, so the one played all the time is easy to find. */}
            {spiritChoices.map(s => <option key={s} value={s}>{recent.includes(s) ? `${s} (recent)` : s}</option>)}
          </select>
        </label>
        {spirit && <img key={spirit} class="spirit-preview" src={spiritImageUrl(spirit)} alt={spirit} onError={e => { e.currentTarget.style.display = 'none'; }} />}
        <label>Board
          <select value={board} onChange={e => setBoard(e.currentTarget.value)}>
            <option value="">(random)</option>
            {options?.boards.map(b => <option key={b} value={b}>{b}</option>)}
          </select>
        </label>
        <label>Adversary
          <select value={adversary} onChange={e => { setAdversary(e.currentTarget.value); setLevel(0); }}>
            <option value="">(none)</option>
            {Object.keys(options?.adversaries ?? {}).map(a => <option key={a} value={a}>{a}</option>)}
          </select>
        </label>
        {adversary && (
          <label>Level
            <select value={level} onChange={e => setLevel(Number(e.currentTarget.value))}>
              {levels.map(l => <option key={l} value={l}>{l}</option>)}
            </select>
          </label>
        )}
        <label>Seed (blank for a random game)
          <input type="number" value={seed} onInput={e => setSeed(e.currentTarget.value)} />
        </label>
        <button type="submit" disabled={busy || !options}>{busy ? 'Starting...' : 'Start'}</button>
      </form>

      {games.length > 0 && (
        <section class="panel">
          <h2>Saved games <span class="muted">(played here in the last month)</span></h2>
          <ul class="saved">
            {games.map(g => (
              <li key={g.gameId}>
                <button type="button" disabled={busy} onClick={() => onOpen(g.gameId)}>
                  {g.spirit} - board {g.board} - seed {g.seed}{g.adversary ? ` - ${g.adversary}` : ''}{g.over ? ' - finished' : ''}
                  <span class="muted"> - {new Date(g.lastPlayed).toLocaleString()}</span>
                </button>
              </li>
            ))}
          </ul>
        </section>
      )}
    </main>
  );
}
