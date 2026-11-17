import type { ComponentChildren } from 'preact';
import type { GameView } from './api/model';
import { InvaderCard } from './icons';

/** The round, fear, blight and invader cards; whatever else is passed goes at the far right of the same line. */
export function StatusBar({ view, children }: { view: GameView; children?: ComponentChildren }) {
  const { fear, blight, invaders } = view;
  const slot = (name: string, cards: string[]) => (
    <span class="inv-slot"><span class="inv-name">{name}</span> {cards.length > 0 ? cards.map((c, i) => <span key={i}>{i > 0 && ', '}<InvaderCard code={c} /></span>) : '-'}</span>
  );
  return (
    <header class="status">
      <span>Round {view.round}</span>
      <span>{view.phase}</span>
      <span>Fear {fear.fearPool}/{fear.fearPoolMax} (terror {fear.terrorLevel})</span>
      <span>Blight on card {blight.blightRemainingOnCard}{blight.cardFlipped && blight.card ? ` - ${blight.card}` : ''}</span>
      {slot('Ravage', invaders.ravage)}
      {slot('Build', invaders.build)}
      {/* No Explore slot: that card is flipped and moved to Build within the Invader Phase, so it is first seen as the Build card. */}
      <span class="inv-slot"><span class="inv-name">Deck</span> {invaders.unrevealedCount}</span>
      <span class="status-end">{children}</span>
    </header>
  );
}
