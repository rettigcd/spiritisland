import { useEffect, useState } from 'preact/hooks';
import { getSetupOptions, listGames } from './api/client';
import type { GameSummary, NewGameRequest, SetupOptions } from './api/model';

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

  const [spirit, setSpirit] = useState('');
  const [board, setBoard] = useState('');
  const [adversary, setAdversary] = useState('');
  const [level, setLevel] = useState(0);
  const [seed, setSeed] = useState('');

  useEffect(() => {
    getSetupOptions().then(r => {
      if (!r.ok) { setLoadError(r.message); return; }
      setOptions(r.value);
      setSpirit(s => s || r.value.spirits[0] || '');
      setBoard(b => b || r.value.boards[0] || '');
    });
    listGames().then(r => { if (r.ok) setGames(r.value); });
  }, []);

  const levels = options?.adversaries[adversary] ?? [];

  const submit = (e: Event) => {
    e.preventDefault();
    const parsed = seed.trim() === '' ? undefined : Number(seed);
    onStart({
      spirit, board,
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
          <select value={spirit} onChange={e => setSpirit(e.currentTarget.value)}>
            {options?.spirits.map(s => <option key={s} value={s}>{s}</option>)}
          </select>
        </label>
        <label>Board
          <select value={board} onChange={e => setBoard(e.currentTarget.value)}>
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
          <h2>Saved games</h2>
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
