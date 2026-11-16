import type { CardView, OptionView, SpiritView } from './api/model';

interface Props {
  spirit: SpiritView;
  isViewer: boolean;
  options: readonly OptionView[];
  busy: boolean;
  onChoose(optionId: string): void;
}

/** The first, plain version: energy, elements and cards. Tracks, growth and innates come with the full panels. */
export function SpiritPanel({ spirit, isViewer, options, busy, onChoose }: Props) {
  // A card (or innate) the current decision is asking about, found by the spirit and the card's title.
  const optionFor = (card: string) => options.find(o => (o.target.kind === 'card' || o.target.kind === 'innate') && o.target.spirit === spirit.id && o.target.card === card);

  const cardList = (title: string, cards: readonly CardView[]) => (
    <div>
      <h3>{title} ({cards.length})</h3>
      <ul class="cards">
        {cards.map(c => {
          const option = optionFor(c.id);
          return (
            <li key={c.id}>
              <button type="button" class={option ? 'choosable' : ''} disabled={busy || !option} onClick={() => option && onChoose(option.id)}>
                {c.name} <span class="muted">${c.cost} {c.speed}{c.elements ? ` ${c.elements}` : ''}</span>
              </button>
            </li>
          );
        })}
      </ul>
    </div>
  );

  return (
    <section class="panel spirit">
      <h2>{spirit.name}{isViewer ? '' : ' (other player)'}</h2>
      <p>Energy {spirit.energy} (+{spirit.presence.energyPerTurn}/turn) · Card plays {spirit.presence.cardPlaysPerTurn} · Elements: {spirit.elements || 'none'}</p>
      <p class="muted">Presence destroyed: {spirit.presence.destroyed} · Sacred sites: {spirit.presence.sacredSites.join(', ') || 'none'}</p>
      {cardList('Hand', spirit.hand)}
      {cardList('In play', spirit.inPlay)}
      <p class="muted">Discard: {spirit.discard.length}</p>
    </section>
  );
}
