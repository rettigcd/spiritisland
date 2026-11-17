import { useEffect, useMemo, useState } from 'preact/hooks';
import type { GameActions, GameState } from './game';
import type { OptionView } from './api/model';
import { Island } from './Island';
import { describeSource, movesBySource } from './moves';
import type { SourceKey } from './moves';
import { LogPanel } from './LogPanel';
import { Menu } from './Menu';
import { Resizer, useSideWidth } from './Resizer';
import { DecisionPanel } from './DecisionPanel';
import { SpiritPanel } from './SpiritPanel';
import { StatusBar } from './StatusBar';

export function GameScreen({ state, actions }: { state: GameState; actions: GameActions }) {
  const [hovered, setHovered] = useState<OptionView | null>(null);
  const [sideWidth, setSideWidth] = useSideWidth();
  const [logOpen, setLogOpen] = useState(false);
  const [picked, setPicked] = useState<SourceKey | null>(null); // where the move being made starts, once the player has picked it
  const { response, staticView, busy } = state;

  // Whatever was picked belongs to the decision it was picked in.
  useEffect(() => { setPicked(null); }, [response]);
  // Escape puts a picked token down.
  useEffect(() => {
    const onKey = (e: KeyboardEvent) => { if (e.key === 'Escape') setPicked(null); };
    window.addEventListener('keydown', onKey);
    return () => window.removeEventListener('keydown', onKey);
  }, []);

  const options = response?.decision?.options ?? [];
  const moves = useMemo(() => movesBySource(options), [response]);
  if (!response) return <main><p>Loading the game...</p></main>;
  const selected = picked !== null && moves.has(picked) ? picked : null;

  // The game's number (its seed) with its spirit, board and adversary is all it takes to play the same game again.
  const setup = staticView?.setup;
  const playAgain = () => {
    if (!setup || !window.confirm('Start this game again from the beginning? This game stays saved.')) return;
    void actions.start({
      spirit: response.view.viewerSpiritId, board: setup.boards[0] ?? 'A', seed: setup.seed,
      adversary: setup.adversary?.name, adversaryLevel: setup.adversary?.level ?? 0,
    });
  };

  return (
    <div class="game">
      <StatusBar view={response.view}>
        <span class="muted" title="The game's number and its seed: starting a game with the same spirit, board, adversary and seed plays it again">
          Game {response.gameId}{setup ? <> / Seed <strong>{setup.seed}</strong></> : null}
        </span>
        <Menu items={[
          { label: 'Back to games', onSelect: actions.leave },
          { label: 'Replay game', title: 'Start this game again from the beginning (same seed)', disabled: !setup || state.busy, onSelect: playAgain },
          { label: 'Log', title: 'What has happened in this game', onSelect: () => setLogOpen(true) },
        ]} />
      </StatusBar>
      {logOpen && <LogPanel log={response.log} onClose={() => setLogOpen(false)} />}


      {state.notice && <p class="notice" role="status">{state.notice} <button type="button" onClick={actions.dismissMessages}>OK</button></p>}
      {state.error && <p class="error" role="alert">{state.error} <button type="button" onClick={actions.dismissMessages}>OK</button></p>}

      <div class="columns" style={{ '--side-width': `${sideWidth}px` }}>
        <div class="main">
          <div class="island-wrap">
            {/* A strip above the board that is always there (empty when nothing is picked up), so showing the hint never moves the
                board while a token is being dragged, and nothing is laid over the board. */}
            <div class="move-hint-bar">
              {selected && (
                <p class="move-hint" role="status">
                  Moving {describeSource(moves.get(selected) ?? [])}: click or drop on a highlighted land.{' '}
                  <button type="button" onClick={() => setPicked(null)}>Cancel</button>
                </p>
              )}
            </div>
            {staticView
              ? <Island staticView={staticView} view={response.view} options={options} hovered={hovered} moves={moves} selected={selected} onSelect={setPicked} disabled={busy} onChoose={actions.choose} />
              : <p>Loading the board...</p>}
          </div>
          <DecisionPanel
            decision={response.decision} result={response.result} picks={state.picks} busy={busy}
            moves={moves} selected={selected} onSelect={setPicked}
            onChoose={actions.choose} onUndo={actions.undo} onHover={setHovered}
          />
        </div>
        <div class="side">
          {response.view.spirits.map(s => (
            <SpiritPanel key={s.id} spirit={s} info={staticView?.spirits.find(x => x.id === s.id)} isViewer={s.id === response.view.viewerSpiritId} options={options} moves={moves} selected={selected} onSelect={setPicked} busy={busy} onChoose={actions.choose} />
          ))}
        </div>
        <Resizer width={sideWidth} onResize={setSideWidth} />
      </div>
    </div>
  );
}
