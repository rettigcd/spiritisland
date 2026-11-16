import type { DecisionView, OptionView, ResultView } from './api/model';

interface Props {
  decision: DecisionView | null;
  result: ResultView | null;
  picks: readonly string[];
  busy: boolean;
  onChoose(optionId: string): void;
  onUndo(): void;
  onHover(option: OptionView | null): void;
}

/**
 * Shows whatever prompt and options arrive; it does not know what a growth phase or a card play is (the server walks the
 * player through the action). Every option is a button here, including the ones that can also be clicked on the island,
 * so nothing is ever out of reach.
 */
export function DecisionPanel({ decision, result, picks, busy, onChoose, onUndo, onHover }: Props) {
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
          <ul class="options">
            {decision.options.map(o => (
              <li key={o.id}>
                <button
                  type="button"
                  disabled={busy}
                  onClick={() => onChoose(o.id)}
                  onMouseEnter={() => onHover(o)}
                  onMouseLeave={() => onHover(null)}
                  onFocus={() => onHover(o)}
                  onBlur={() => onHover(null)}
                >
                  {o.label}
                </button>
              </li>
            ))}
          </ul>
        </>
      )}

      <div class="picks">
        <button type="button" disabled={busy || picks.length === 0} onClick={onUndo}>Undo</button>
        {picks.length > 0 && <span class="pick-trail">This action so far: {picks.join(' → ')}</span>}
      </div>
    </section>
  );
}
