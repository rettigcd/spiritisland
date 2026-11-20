import type { ComponentChildren } from 'preact';
import { useState } from 'preact/hooks';
import type { CardView, InnateStatic, OptionView, SlotView, SpiritStatic, SpiritView } from './api/model';
import { destroyedSource, slotSource, sourceOf } from './moves';
import type { OptionTarget } from './api/model';
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
  /** Picks an action (a card, an innate power, a growth action) to use on a target; with one target it is simply chosen. */
  onPickSource(source: SourceKey): void;
  /** Pointing at an action shows what it could target. */
  onHoverSource(source: SourceKey | null): void;
  /** The chosen action targets spirits, and this one is among them: shown highlighted. */
  reached: boolean;
  /** The picked action's option for this spirit as its target: choosing it uses the action on this spirit. */
  targetOption?: OptionView;
  busy: boolean;
  onChoose(optionId: string): void;
}

type TrackName = 'energy' | 'cardPlays';

/**
 * Everything about one spirit: its presence tracks, growth, innate powers, special rules and cards. Whatever the current
 * decision is asking about (a track slot, a card, an innate power) is highlighted and can be clicked here as well as in the
 * decision panel.
 */
export function SpiritPanel({ spirit, info, isViewer, options, moves, selected, onSelect, onPickSource, onHoverSource, reached, targetOption, busy, onChoose }: Props) {
  const optionWhere = (kinds: readonly string[], match: (o: OptionView) => boolean) =>
    options.find(o => kinds.includes(o.target.kind) && o.target.spirit === spirit.id && match(o));

  const slotOption = (track: TrackName, slot: number) => optionWhere(['track', 'trackPresence'], o => o.target.track === track && o.target.slot === slot);
  const cardOption = (card: string) => optionWhere(['card'], o => o.target.card === card);
  const innateOption = (name: string) => optionWhere(['innate'], o => o.target.card === name);
  // A growth action is picked by which action of which group it is (several can read the same, like two "Place Presence (1)").
  const growthOption = (group: number, action: number) => optionWhere(['growth'], o => o.target.group === group && o.target.action === action);

  // An action offered with its targets is picked (then its target) and pointing at it shows what it could target.
  const actionStart = (match: (t: OptionTarget) => boolean): SourceKey | undefined => {
    const o = options.find(o => o.target.kind === 'act' && o.target.spirit === spirit.id && match(o.target));
    return o ? sourceOf(o.target) ?? undefined : undefined;
  };
  const cardStart = (card: string) => actionStart(t => t.card === card && t.group == null);
  const growthStart = (group: number, action: number) => actionStart(t => t.group === group && t.action === action);
  const useStart = (source: SourceKey) => () => { if (!busy) onPickSource(source); };
  const pointing = (source: SourceKey) => ({
    onMouseEnter: () => onHoverSource(source), onMouseLeave: () => onHoverSource(null),
    onFocus: () => onHoverSource(source), onBlur: () => onHoverSource(null),
  });

  const choose = (o: OptionView | undefined) => () => { if (o && !busy) onChoose(o.id); };
  const pick = (source: SourceKey) => () => { if (!busy) onSelect(selected === source ? null : source); };
  const pile = destroyedSource(spirit.id);

  return (
    <section class={`panel spirit ${reached ? 'reached' : ''}`} style={{ '--spirit-image': `url("${spiritImageUrl(spirit.name)}")` }}>
      <h2>
        {spirit.name}{isViewer ? '' : ' (other player)'}
        {targetOption && <button type="button" class="choosable target-spirit" disabled={busy} onClick={choose(targetOption)}>Use it on this spirit</button>}
      </h2>
      {info && (
        <>
          <h3>Growth</h3>
          <ul class="growth">
            {info.growthGroups.map((g, gi) => (
              <li key={g.id} class={spirit.growthGroupsUsed.includes(g.id) ? 'used' : ''}>
                {g.actions.map((action, ai) => {
                  const option = growthOption(gi, ai);
                  const start = growthStart(gi, ai);
                  return (
                    <span key={ai}>
                      {ai > 0 && ' · '}
                      {start
                        ? <button type="button" class={`choosable growth-action ${selected === start ? 'selected' : ''}`} disabled={busy} onClick={useStart(start)} {...pointing(start)}>{action}</button>
                        : option
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
              option={innateOption(innate.name)} start={actionStart(t => t.card === innate.name && t.group == null)} selected={selected}
              useStart={useStart} pointing={pointing} busy={busy} choose={choose} />
          ))}
        </>
      )}

      <CardList title="In play" cards={spirit.inPlay} optionFor={cardOption} startFor={cardStart} selected={selected} useStart={useStart} pointing={pointing} busy={busy} choose={choose} />
      {/* The hand and the discard take a lot of room and are rarely what is being acted on, so they show as a count that opens on a click,
          and open by themselves when the decision is about one of their cards (playing cards, reclaiming, forgetting). */}
      <Collapsible title="Hand" count={spirit.hand.length} wanted={spirit.hand.some(c => cardOption(c.id) || cardStart(c.id))}>
        <CardList title="" cards={spirit.hand} optionFor={cardOption} startFor={cardStart} selected={selected} useStart={useStart} pointing={pointing} busy={busy} choose={choose} />
      </Collapsible>
      <Collapsible title="Discard" count={spirit.discard.length} wanted={spirit.discard.some(c => cardOption(c.id) || cardStart(c.id))}>
        <CardList title="" cards={spirit.discard} optionFor={cardOption} startFor={cardStart} selected={selected} useStart={useStart} pointing={pointing} busy={busy} choose={choose} />
      </Collapsible>
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

/** A list that is closed (just its name and how many) unless the player opens it, or the current decision is about something in it. */
function Collapsible({ title, count, wanted, children }: { title: string; count: number; wanted: boolean; children: ComponentChildren }) {
  const [opened, setOpened] = useState(false);
  return (
    <details class="collapsible" open={wanted || opened} onToggle={e => { if (!wanted) setOpened(e.currentTarget.open); }}>
      <summary>{title} ({count}){wanted ? ' - choose one' : ''}</summary>
      {children}
    </details>
  );
}

interface PickProps {
  /** Set when the power is offered with its targets: the start of its choices. */
  selected: SourceKey | null;
  useStart(source: SourceKey): () => void;
  pointing(source: SourceKey): Pointing;
}

/** The handlers that make pointing at (or focusing) an action show what it could target. */
interface Pointing { onMouseEnter(): void; onMouseLeave(): void; onFocus(): void; onBlur(): void }

interface InnateProps extends PickProps {
  start: SourceKey | undefined;
  innate: InnateStatic;
  /** For each tier: "Yes" if this turn's elements meet it, "AsPrepared" if they could with the elements being prepared, otherwise "No". */
  status: readonly string[];
  option: OptionView | undefined;
  busy: boolean;
  choose(o: OptionView | undefined): () => void;
}

function Innate({ innate, status, option, start, selected, useStart, pointing, busy, choose }: InnateProps) {
  return (
    <div class="innate">
      <h4 title={innate.instructions}>
        {innate.name}
        <span class="muted"> <Speed speed={innate.speed} /> · range {rangeText(innate.range)} · {innate.target}</span>
        {start
          ? <button type="button" class={`choosable ${selected === start ? 'selected' : ''}`} disabled={busy} onClick={useStart(start)} {...pointing(start)}>Use</button>
          : option && <button type="button" class="choosable" disabled={busy} onClick={choose(option)}>Use</button>}
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

interface CardListProps extends PickProps {
  title: string;
  cards: readonly CardView[];
  optionFor(card: string): OptionView | undefined;
  startFor(card: string): SourceKey | undefined;
  busy: boolean;
  choose(o: OptionView | undefined): () => void;
}

function CardList({ title, cards, optionFor, startFor, selected, useStart, pointing, busy, choose }: CardListProps) {
  return (
    <div>
      {title && <h3>{title} ({cards.length})</h3>}
      <ul class="cards">
        {cards.map(c => {
          const option = optionFor(c.id);
          const start = startFor(c.id);
          return (
            <li key={c.id}>
              <button
                type="button" class={`${option || start ? 'choosable' : ''} ${start && selected === start ? 'selected' : ''}`} disabled={busy || !(option || start)}
                onClick={start ? useStart(start) : choose(option)} {...(start ? pointing(start) : {})}
                title={`${c.name}: range ${rangeText(c.range)} · ${c.target}. ${c.instructions}`}
              >
                <Coin value={c.cost} title="energy to play" speed={c.speed} /> {c.name} <Elements text={c.elements} />
              </button>
            </li>
          );
        })}
      </ul>
    </div>
  );
}
