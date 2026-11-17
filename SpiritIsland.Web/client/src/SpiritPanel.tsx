import type { CardView, InnateStatic, OptionView, SlotView, SpiritStatic, SpiritView } from './api/model';
import { destroyedSource, slotSource } from './moves';
import type { SourceKey } from './moves';
import { rangeText } from './text';
import { Coin, Elements, Speed } from './icons';
import { spiritImageUrl } from './spiritImage';

interface Props {
  spirit: SpiritView;
  /** The spirit's tracks, growth, innate powers and rules. Undefined until the static view has loaded. */
  info: SpiritStatic | undefined;
  isViewer: boolean;
  options: readonly OptionView[];
  /** The decision's moves by where they start, and the start the player has picked: a track slot or the destroyed pile can be one. */
  moves: ReadonlyMap<SourceKey, readonly OptionView[]>;
  selected: SourceKey | null;
  onSelect(source: SourceKey | null): void;
  busy: boolean;
  onChoose(optionId: string): void;
}

type TrackName = 'energy' | 'cardPlays';

/**
 * Everything about one spirit: its presence tracks, growth, innate powers, special rules and cards. Whatever the current
 * decision is asking about (a track slot, a card, an innate power) is highlighted and can be clicked here as well as in the
 * decision panel.
 */
export function SpiritPanel({ spirit, info, isViewer, options, moves, selected, onSelect, busy, onChoose }: Props) {
  const optionWhere = (kinds: readonly string[], match: (o: OptionView) => boolean) =>
    options.find(o => kinds.includes(o.target.kind) && o.target.spirit === spirit.id && match(o));

  const slotOption = (track: TrackName, slot: number) => optionWhere(['track', 'trackPresence'], o => o.target.track === track && o.target.slot === slot);
  const cardOption = (card: string) => optionWhere(['card'], o => o.target.card === card);
  const innateOption = (name: string) => optionWhere(['innate'], o => o.target.card === name);
  // A growth action is picked by which action of which group it is (several can read the same, like two "Place Presence (1)").
  const growthOption = (group: number, action: number) => optionWhere(['growth'], o => o.target.group === group && o.target.action === action);

  const choose = (o: OptionView | undefined) => () => { if (o && !busy) onChoose(o.id); };
  const pick = (source: SourceKey) => () => { if (!busy) onSelect(selected === source ? null : source); };
  const pile = destroyedSource(spirit.id);

  return (
    <section class="panel spirit" style={{ '--spirit-image': `url("${spiritImageUrl(spirit.name)}")` }}>
      <h2>{spirit.name}{isViewer ? '' : ' (other player)'}</h2>
      {info && (
        <>
          <h3>Growth</h3>
          <ul class="growth">
            {info.growthGroups.map((g, gi) => (
              <li key={g.id} class={spirit.growthGroupsUsed.includes(g.id) ? 'used' : ''}>
                {g.actions.map((action, ai) => {
                  const option = growthOption(gi, ai);
                  return (
                    <span key={ai}>
                      {ai > 0 && ' · '}
                      {option
                        ? <button type="button" class="choosable growth-action" disabled={busy} onClick={choose(option)}>{action}</button>
                        : action}
                    </span>
                  );
                })}
                {g.energyChange !== 0 ? <span class="muted"> ({g.energyChange > 0 ? '+' : ''}{g.energyChange} energy)</span> : null}
              </li>
            ))}
          </ul>
        </>
      )}
      <p>
        Energy <Coin value={spirit.energy} /> (+{spirit.presence.energyPerTurn}/turn) · Card plays {spirit.presence.cardPlaysPerTurn}
        {' '}· Elements: <Elements text={spirit.elements} />
      </p>
      <p class="muted">Presence destroyed: {spirit.presence.destroyed} · Sacred sites: {spirit.presence.sacredSites.join(', ') || 'none'}</p>
      {moves.has(pile) && (
        <p>
          <button type="button" class={`movable ${selected === pile ? 'selected' : ''}`} disabled={busy} onClick={pick(pile)}>
            Place destroyed presence ({spirit.presence.destroyed} in the pile)
          </button>
        </p>
      )}

      {info && (
        <>
          <Track title="Energy track" track="energy" slots={info.energyTrack} revealed={spirit.presence.energy.revealed} optionFor={slotOption} spirit={spirit.id} moves={moves} selected={selected} pick={pick} busy={busy} choose={choose} />
          <Track title="Card plays track" track="cardPlays" slots={info.cardPlayTrack} revealed={spirit.presence.cardPlays.revealed} optionFor={slotOption} spirit={spirit.id} moves={moves} selected={selected} pick={pick} busy={busy} choose={choose} />

          <h3>Innate powers</h3>
          {info.innates.map(innate => (
            <Innate key={innate.name} innate={innate} status={spirit.innates.find(i => i.name === innate.name)?.tierStatus ?? []}
              option={innateOption(innate.name)} busy={busy} choose={choose} />
          ))}
        </>
      )}

      <CardList title="Hand" cards={spirit.hand} optionFor={cardOption} busy={busy} choose={choose} />
      <CardList title="In play" cards={spirit.inPlay} optionFor={cardOption} busy={busy} choose={choose} />
      <details>
        <summary>Discard ({spirit.discard.length})</summary>
        <CardList title="" cards={spirit.discard} optionFor={cardOption} busy={busy} choose={choose} />
      </details>
      {info && info.specialRules.length > 0 && (
        <details class="rules">
          <summary>Special rules</summary>
          {info.specialRules.map(r => <p key={r.title}><strong>{r.title}.</strong> {r.description}</p>)}
        </details>
      )}
    </section>
  );
}

interface TrackProps {
  title: string;
  track: TrackName;
  slots: readonly SlotView[];
  /** Indexes of the slots whose presence has been taken: those are the ones whose benefit you now have. */
  revealed: readonly number[];
  optionFor(track: TrackName, slot: number): OptionView | undefined;
  spirit: string;
  moves: ReadonlyMap<SourceKey, readonly OptionView[]>;
  selected: SourceKey | null;
  pick(source: SourceKey): () => void;
  busy: boolean;
  choose(o: OptionView | undefined): () => void;
}

function Track({ title, track, slots, revealed, optionFor, spirit, moves, selected, pick, busy, choose }: TrackProps) {
  return (
    <div class="track">
      <h3>{title}</h3>
      <ol class="slots">
        {slots.map((slot, i) => {
          const open = revealed.includes(i);
          const option = optionFor(track, i);
          const source = slotSource(spirit, track, i);
          const movable = !option && moves.has(source); // presence that can be moved from this slot to a land
          const classes = `slot ${open ? 'revealed' : 'covered'} ${option ? 'choosable' : ''} ${movable ? 'movable' : ''} ${selected === source ? 'selected' : ''}`;
          const hint = open ? 'Revealed' : 'Presence still here';
          return (
            <li key={i}>
              {option
                ? <button type="button" class={classes} disabled={busy} onClick={choose(option)} title={hint}>{slot.label}</button>
                : movable
                  ? <button type="button" class={classes} disabled={busy} onClick={pick(source)} title="Pick up this presence, then choose a land">{slot.label}</button>
                  : <span class={classes} title={hint}>{slot.label}</span>}
            </li>
          );
        })}
      </ol>
    </div>
  );
}

interface InnateProps {
  innate: InnateStatic;
  /** For each tier: "Yes" if this turn's elements meet it, "AsPrepared" if they could with the elements being prepared, otherwise "No". */
  status: readonly string[];
  option: OptionView | undefined;
  busy: boolean;
  choose(o: OptionView | undefined): () => void;
}

function Innate({ innate, status, option, busy, choose }: InnateProps) {
  return (
    <div class="innate">
      <h4 title={innate.instructions}>
        {innate.name}
        <span class="muted"> <Speed speed={innate.speed} /> · range {rangeText(innate.range)} · {innate.target}</span>
        {option && <button type="button" class="choosable" disabled={busy} onClick={choose(option)}>Use</button>}
      </h4>
      {/* Which tiers are met is what matters while acting, so that stays on screen; what each one does is in its tooltip. */}
      <ul class="tiers">
        {innate.tiers.map((t, i) => (
          <li key={i} class={`tier tier-${(status[i] ?? 'No').toLowerCase()}`} title={t.effect}>
            <Elements text={t.threshold} />
          </li>
        ))}
      </ul>
    </div>
  );
}

interface CardListProps {
  title: string;
  cards: readonly CardView[];
  optionFor(card: string): OptionView | undefined;
  busy: boolean;
  choose(o: OptionView | undefined): () => void;
}

function CardList({ title, cards, optionFor, busy, choose }: CardListProps) {
  return (
    <div>
      {title && <h3>{title} ({cards.length})</h3>}
      <ul class="cards">
        {cards.map(c => {
          const option = optionFor(c.id);
          return (
            <li key={c.id}>
              <button
                type="button" class={option ? 'choosable' : ''} disabled={busy || !option} onClick={choose(option)}
                title={`${c.name}: range ${rangeText(c.range)} · ${c.target}. ${c.instructions}`}
              >
                {c.name} <span class="muted"><Coin value={c.cost} title="energy to play" /> <Speed speed={c.speed} /> <Elements text={c.elements} /></span>
              </button>
            </li>
          );
        })}
      </ul>
    </div>
  );
}
