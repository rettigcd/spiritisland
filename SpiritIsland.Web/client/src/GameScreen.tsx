import { useState } from 'preact/hooks';
import type { GameActions, GameState } from './game';
import type { OptionView } from './api/model';
import { Island } from './Island';
import { DecisionPanel } from './DecisionPanel';
import { SpiritPanel } from './SpiritPanel';
import { StatusBar } from './StatusBar';

export function GameScreen({ state, actions }: { state: GameState; actions: GameActions }) {
  const [hovered, setHovered] = useState<OptionView | null>(null);
  const { response, staticView, busy } = state;
  if (!response) return <main><p>Loading the game...</p></main>;

  const options = response.decision?.options ?? [];

  return (
    <div class="game">
      <div class="topbar">
        <button type="button" onClick={actions.leave}>Back to games</button>
        <span class="muted">Game {response.gameId}</span>
      </div>

      <StatusBar view={response.view} />

      {state.notice && <p class="notice" role="status">{state.notice} <button type="button" onClick={actions.dismissMessages}>OK</button></p>}
      {state.error && <p class="error" role="alert">{state.error} <button type="button" onClick={actions.dismissMessages}>OK</button></p>}

      <div class="columns">
        <div class="island-wrap">
          {staticView
            ? <Island staticView={staticView} view={response.view} options={options} hovered={hovered} disabled={busy} onChoose={actions.choose} />
            : <p>Loading the board...</p>}
        </div>
        <div class="side">
          <DecisionPanel
            decision={response.decision} result={response.result} picks={state.picks} busy={busy}
            onChoose={actions.choose} onUndo={actions.undo} onHover={setHovered}
          />
          {response.view.spirits.map(s => (
            <SpiritPanel key={s.id} spirit={s} isViewer={s.id === response.view.viewerSpiritId} options={options} busy={busy} onChoose={actions.choose} />
          ))}
        </div>
      </div>
    </div>
  );
}
