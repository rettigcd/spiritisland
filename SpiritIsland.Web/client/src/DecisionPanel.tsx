import type { DecisionView, OptionView, ResultView } from './api/model';
import { rangeText } from './text';
import { describeDestination, describeSource, isAction, sourceOf } from './moves';
import type { SourceKey } from './moves';

interface Props {
  decision: DecisionView | null;
  result: ResultView | null;
  picks: readonly string[];
  busy: boolean;
  moves: ReadonlyMap<SourceKey, OptionView[]>; // the decision's moves and actions, grouped by where they start
  selected: SourceKey | null; // the start of the move or action being made, if one has been picked
  onSelect(source: SourceKey | null): void;
  /** Picks where a move or an action starts (an action with one target is simply chosen). */
  onPickSource(source: SourceKey): void;
  /** Pointing at a start (or leaving it): what it could reach is shown on the board and the spirit panels. */
  onHoverSource(source: SourceKey | null): void;
  onChoose(optionId: string): void;
  onUndo(): void;
  onHover(option: OptionView | null): void;
}

/** What an option that is a card says about itself, shown when the pointer rests on it. */
function cardDetails(o: OptionView): string | undefined {
  const c = o.card;
  return c ? `${c.name}: ${c.elements ? `${c.elements} · ` : ''}range ${rangeText(c.range)} · ${c.target}. ${c.instructions}` : undefined;
}

/**
 * Shows whatever prompt and options arrive; it does not know what a growth phase or a card play is (the server walks the
 * player through the action). Every option is a button here, including the ones that can also be clicked on the island,
 * so nothing is ever out of reach.
 */
export function DecisionPanel({ decision, result, picks, busy, moves, selected, onSelect, onPickSource, onHoverSource, onChoose, onUndo, onHover }: Props) {
  // Moves are offered in two steps: first where the move starts (one entry per start), then where it goes.
  const destinations = selected !== null ? moves.get(selected) : undefined;
  const seen = new Set<SourceKey>();
  return (
    <section class="panel decision">
      {result && (
        <div class={`result ${result.outcome.toLowerCase()}`}>
          <strong>{result.outcome}</strong>: {result.cause}
        </div>
      )}

      {decision && (
        <>
          <h2>{decision.prompt}</h2>
          {decision.details && <p class="muted details">{decision.details}</p>}
          {destinations && (
            <>
              <p class="muted details">{isAction(selected ?? '') ? 'Use' : 'Move'} {describeSource(destinations)} {isAction(selected ?? '') ? 'on' : 'to'}:</p>
              <ul class="options">
                {destinations.map(o => (
                  <li key={o.id}>
                    <button
                      type="button" disabled={busy} onClick={() => onChoose(o.id)}
                      onMouseEnter={() => onHover(o)} onMouseLeave={() => onHover(null)} onFocus={() => onHover(o)} onBlur={() => onHover(null)}
                    >
                      {describeDestination(o)}
                    </button>
                  </li>
                ))}
                <li><button type="button" disabled={busy} onClick={() => onSelect(null)}>Back</button></li>
              </ul>
            </>
          )}
          {!destinations && <ul class="options">
            {decision.options.map(o => {
              const source = sourceOf(o.target);
              if (source !== null) {
                if (seen.has(source)) return null; // listed once, at its first move
                seen.add(source);
                return (
                  <li key={source}>
                    <button
                      type="button" disabled={busy} onClick={() => onPickSource(source)} title={cardDetails(o)}
                      onMouseEnter={() => onHoverSource(source)} onMouseLeave={() => onHoverSource(null)}
                      onFocus={() => onHoverSource(source)} onBlur={() => onHoverSource(null)}
                    >
                      {describeSource(moves.get(source) ?? [o])}
                    </button>
                  </li>
                );
              }
              return (
              <li key={o.id}>
                <button
                  type="button"
                  disabled={busy}
                  title={cardDetails(o)}
                  onClick={() => onChoose(o.id)}
                  onMouseEnter={() => onHover(o)}
                  onMouseLeave={() => onHover(null)}
                  onFocus={() => onHover(o)}
                  onBlur={() => onHover(null)}
                >
                  {o.label}
                </button>
              </li>
              );
            })}
          </ul>}
        </>
      )}

      <div class="picks">
        <button type="button" disabled={busy || picks.length === 0} onClick={onUndo}>Undo</button>
        {picks.length > 0 && <span class="pick-trail">This action so far: {picks.join(' → ')}</span>}
      </div>
    </section>
  );
}
