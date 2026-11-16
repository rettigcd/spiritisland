import type { GameView } from './api/model';

export function StatusBar({ view }: { view: GameView }) {
  const { fear, blight, invaders } = view;
  const slot = (name: string, cards: string[]) => (
    <span class="inv-slot"><span class="inv-name">{name}</span> {cards.length > 0 ? cards.join(', ') : '-'}</span>
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
    </header>
  );
}
