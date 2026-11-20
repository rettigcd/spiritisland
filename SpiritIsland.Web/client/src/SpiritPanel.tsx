import type { ComponentChildren } from 'preact';
import { useEffect, useRef, useState } from 'preact/hooks';
import type { CardView, InnateStatic, OptionView, SlotView, SpiritStatic, SpiritView } from './api/model';
import { destinationsOf, destroyedSource, slotSource, sourceOf } from './moves';
import { badgeUrl, PRESENCE_IMAGE, presenceHue, presenceImageUrl, trackImageUrl } from './images';
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
  const useStart = (source: SourceKey) => () => { if (!busy && !justDragged.current) onPickSource(source); };
  // Pointing at an action shows what it could target; pressing and moving drags it onto one of them (a land, or a spirit's badge).
  const pointing = (source: SourceKey) => ({
    onMouseEnter: () => onHoverSource(source), onMouseLeave: () => onHoverSource(null),
    onFocus: () => onHoverSource(source), onBlur: () => onHoverSource(null),
    onPointerDown: drag(source, true),
  });

  const choose = (o: OptionView | undefined) => () => { if (o && !busy && !justDragged.current) onChoose(o.id); };
  // A card that is simply chosen (played, discarded) is dragged onto the area it goes to: 'inPlay' for a card from the hand, 'discard' for one in play or in hand.
  const dragCard = (o: OptionView | undefined, to: readonly string[]) => o ? drag(null, true, { option: o, to }) : undefined;
  const rulesDialog = useRef<HTMLDialogElement | null>(null);
  // Cards this spirit is offered that it does not have yet: drawn, so they appear only in the draw area.
  const owned = new Set([...spirit.hand, ...spirit.inPlay, ...spirit.discard].map(c => c.id));
  const drawn: CardView[] = [];
  for (const o of options)
    if (o.target.kind === 'card' && o.target.spirit === spirit.id && o.card && !owned.has(o.card.id) && !drawn.some(c => c.id === o.card!.id)) drawn.push(o.card);
  const ignoreClick = () => justDragged.current;
  const pick = (source: SourceKey) => () => { if (!busy && !justDragged.current) onSelect(selected === source ? null : source); };

  // ---- Presence picked up from a track (or the destroyed pile) can be dragged onto a land it can go to. Press, move, release on a land:
  // ---- it is placed there; released anywhere else, it is put down. A press and release without moving is a click (pick, then click a land).
  const [carrying, setCarrying] = useState(false);
  const carriedPicture = useRef<HTMLImageElement | null>(null);
  const pointer = useRef({ x: 0, y: 0 });
  // The picture under the pointer is moved directly, on every pointer move, rather than by re-drawing the panel.
  const placeCarried = () => {
    const el = carriedPicture.current;
    if (!el) return;
    el.style.left = `${pointer.current.x}px`;
    el.style.top = `${pointer.current.y}px`;
    if (el.dataset.hue) el.style.filter = `hue-rotate(${el.dataset.hue}deg) drop-shadow(0 3px 4px rgba(0, 0, 0, .5))`; // the red picture, turned to the spirit's colour
  };
  const justDragged = useRef(false); // the click that can follow the end of a drag is not a pick
  const stopCarrying = useRef<(() => void) | null>(null);
  useEffect(() => () => stopCarrying.current?.(), []);
  /** The spirit badge (anywhere on the page) under the pointer, if any: judged by where it is drawn, because it sits behind the panel's other things. */
  const badgeUnder = (x: number, y: number) =>
    [...document.querySelectorAll<HTMLElement>('[data-drop-spirit]')].find(b => {
      const r = b.getBoundingClientRect();
      return r.left <= x && x <= r.right && r.top <= y && y <= r.bottom;
    });
  /** This spirit's card area (In play, Discard) under the pointer, if any. */
  const zoneUnder = (from: HTMLElement, x: number, y: number) =>
    [...(from.closest('.spirit')?.querySelectorAll<HTMLElement>('[data-drop-zone]') ?? [])].find(z => {
      const r = z.getBoundingClientRect();
      return r.left <= x && x <= r.right && r.top <= y && y <= r.bottom;
    });
  /**
   * @param copyOfButton what is carried: a copy of the button pressed (an action), or, if false, the spirit's presence.
   * @param choice for a card that is chosen rather than aimed: the option, and the areas it can be dropped on to choose it.
   */
  const drag = (source: SourceKey | null, copyOfButton = false, choice?: { option: OptionView; to: readonly string[] }) => (e: PointerEvent) => {
    if (busy || e.button !== 0) return;
    e.preventDefault(); // no text selection while dragging
    const startX = e.clientX, startY = e.clientY;
    let moved = false;
    let copy: HTMLElement | null = null;
    const button = e.currentTarget as HTMLElement;
    const onMove = (ev: PointerEvent) => {
      if (!moved && Math.hypot(ev.clientX - startX, ev.clientY - startY) > 5) {
        moved = true;
        if (source) onSelect(source); // picked, so the lands (and spirits) it can reach show
        if (choice) button.closest('.spirit')?.querySelectorAll<HTMLElement>('[data-drop-zone]').forEach(z => z.classList.toggle('drop-ready', choice.to.includes(z.dataset.dropZone!)));
        if (copyOfButton) {
          copy = button.cloneNode(true) as HTMLElement;
          copy.classList.add('carried');
          button.closest('.spirit')?.appendChild(copy);
        }
      }
      pointer.current = { x: ev.clientX, y: ev.clientY };
      if (copy) { copy.style.left = `${ev.clientX}px`; copy.style.top = `${ev.clientY}px`; }
      if (moved && !copyOfButton) { setCarrying(true); placeCarried(); }
      if (moved) {
        const over = badgeUnder(ev.clientX, ev.clientY);
        document.querySelectorAll('[data-drop-spirit]').forEach(b => b.classList.toggle('drop-over', b === over));
        const zone = choice ? zoneUnder(button, ev.clientX, ev.clientY) : undefined;
        button.closest('.spirit')?.querySelectorAll('[data-drop-zone]').forEach(z => z.classList.toggle('drop-over', z === zone && choice!.to.includes(z.getAttribute('data-drop-zone')!)));
      }
    };
    const finish = () => {
      window.removeEventListener('pointermove', onMove);
      window.removeEventListener('pointerup', onUp);
      window.removeEventListener('pointercancel', onCancel);
      stopCarrying.current = null;
      copy?.remove();
      document.querySelectorAll('.drop-over, .drop-ready').forEach(b => b.classList.remove('drop-over', 'drop-ready'));
      setCarrying(false);
    };
    const onUp = (ev: PointerEvent) => {
      finish();
      if (!moved) return;
      justDragged.current = true;
      setTimeout(() => { justDragged.current = false; }, 0);
      if (choice) {
        const target = zoneUnder(button, ev.clientX, ev.clientY);
        const zone = target?.getAttribute('data-drop-zone');
        if (target && zone && choice.to.includes(zone)) {
          target.dispatchEvent(new Event('zone-open')); // the card dropped there is shown, so a folded area opens
          onChoose(choice.option.id);
        }
        return;
      }
      const ways = (source && moves.get(source)) || [];
      const land = document.elementFromPoint(ev.clientX, ev.clientY)?.closest('[data-space]')?.getAttribute('data-space');
      const spiritId = badgeUnder(ev.clientX, ev.clientY)?.getAttribute('data-drop-spirit');
      const move = land ? destinationsOf(ways).get(land) : spiritId ? ways.find(o => o.target.toSpirit === spiritId) : undefined;
      if (move) onChoose(move.id); else onSelect(null);
    };
    const onCancel = () => { finish(); if (moved) onSelect(null); };
    window.addEventListener('pointermove', onMove);
    window.addEventListener('pointerup', onUp);
    window.addEventListener('pointercancel', onCancel);
    stopCarrying.current = finish;
  };
  const presenceTokenImage = `Presence:${spirit.id}`;
  const ownPresencePicture = presenceImageUrl(presenceTokenImage);
  const pile = destroyedSource(spirit.id);

  return (
    <section class={`panel spirit ${reached ? 'reached' : ''}`} style={{ '--spirit-image': `url("${spiritImageUrl(spirit.name)}")` }}>
      {/* The spirit's badge, behind everything else: where a power that targets this spirit is dropped. */}
      <img class={`spirit-badge ${reached ? 'ready' : ''}`} src={badgeUrl(spirit.name)} alt="" title={spirit.name} width={72} height={72} draggable={false} data-drop-spirit={spirit.id} />
      <h2>
        {spirit.name}{isViewer ? '' : ' (other player)'}
        {info && info.specialRules.length > 0 && <button type="button" class="rules-button" onClick={() => rulesDialog.current?.showModal()} title="This spirit's special rules">✦ Special rules</button>}
      </h2>
      {/* Out of the flow, below the badge, so showing it moves nothing: the click alternative to dropping the power on the badge. */}
      {targetOption && <button type="button" class="choosable target-spirit" disabled={busy} onClick={choose(targetOption)} title="Use the picked power on this spirit">Use here</button>}
      {info && (
        <>
          <h3 title={info.growthPicksNote ? `Pick: ${info.growthPicksNote}` : undefined}>Growth (pick {info.growthPicks})</h3>
          <ul class="growth">
            {info.growthGroups.map((g, gi) => (
              <li key={g.id} class={spirit.growthGroupsUsed.includes(g.id) ? 'used' : ''}>
                <span class="growth-label" title={`Growth ${gi + 1}`}>G{gi + 1}</span>
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
                          : <span class="growth-action plain">{action}</span>}
                    </span>
                  );
                })}
                {g.energyChange !== 0 ? <span class="muted"> ({g.energyChange > 0 ? '+' : ''}{g.energyChange} energy)</span> : null}
              </li>
            ))}
          </ul>
        </>
      )}
      {/* Always there, even with no elements, so what is below it does not move as elements come and go. */}
      <p class="elements-row">{spirit.elements && <Elements text={spirit.elements} />}</p>

      {info && (
        <>
          {/* Beside the tracks: the energy there is now, and the presence that has been destroyed (click it to place it). */}
          <div class="track-row">
            <Track title="Energy" track="energy" slots={info.energyTrack} revealed={spirit.presence.energy.revealed} optionFor={slotOption} spirit={spirit.id} moves={moves} selected={selected} pick={pick} drag={drag} busy={busy} choose={choose} />
            <div class="track-side big-coin"><Coin value={spirit.energy} /></div>
          </div>
          <div class="track-row">
            <Track title={`Card plays: ${spirit.presence.cardPlaysPerTurn}`} track="cardPlays" slots={info.cardPlayTrack} revealed={spirit.presence.cardPlays.revealed} optionFor={slotOption} spirit={spirit.id} moves={moves} selected={selected} pick={pick} drag={drag} busy={busy} choose={choose} />
            <div class="track-side">
              {0 < spirit.presence.destroyed && (() => {
                const picture = <><img src="/img/icons/destroyed.png" alt="" width={54} height={54} /><span class="destroyed-count">{spirit.presence.destroyed}</span></>;
                return moves.has(pile)
                  ? <button type="button" class={`destroyed-pile pickable ${selected === pile ? 'selected' : ''}`} disabled={busy} onClick={pick(pile)} onPointerDown={drag(pile)} title={`Place destroyed presence (${spirit.presence.destroyed} in the pile), then choose a land`}>{picture}</button>
                  : <span class="destroyed-pile" title={`${spirit.presence.destroyed} presence destroyed`}>{picture}</span>;
              })()}
            </div>
          </div>

          <h3>Innate powers</h3>
          {info.innates.map(innate => (
            <Innate key={innate.name} innate={innate} status={spirit.innates.find(i => i.name === innate.name)?.tierStatus ?? []}
              option={innateOption(innate.name)} start={actionStart(t => t.card === innate.name && t.group == null)} selected={selected}
              useStart={useStart} pointing={pointing} busy={busy} choose={choose} />
          ))}
        </>
      )}

      {/* The card areas are always on screen, full width, with their name in the background and their cards to the right. Click the empty part
          of an area to fold or open it (an empty area stays folded; a folded one shows how many cards it holds). One opens by itself when the
          decision is about one of its cards. A card from the hand is dragged onto In play to play it; a card offered from the draw area onto the Hand. */}
      <Zone zone="inPlay" count={spirit.inPlay.length} wanted={spirit.inPlay.some(c => cardOption(c.id) || cardStart(c.id))} ignoreClick={ignoreClick}>
        <CardList cards={spirit.inPlay} optionFor={cardOption} startFor={cardStart} selected={selected} useStart={useStart} pointing={pointing} busy={busy} choose={choose} dragCard={o => dragCard(o, ['discard'])} />
      </Zone>
      <Zone zone="hand" count={spirit.hand.length} wanted={spirit.hand.some(c => cardOption(c.id) || cardStart(c.id))} startOpen ignoreClick={ignoreClick}>
        <CardList cards={spirit.hand} optionFor={cardOption} startFor={cardStart} selected={selected} useStart={useStart} pointing={pointing} busy={busy} choose={choose} dragCard={o => dragCard(o, ['inPlay', 'discard'])} />
      </Zone>
      {/* Cards newly brought into the game (drawn from the major or minor deck) and offered to this spirit: not in any other area. */}
      {drawn.length > 0 && (
        <Zone zone="draw" count={drawn.length} wanted startOpen ignoreClick={ignoreClick}>
          <CardList cards={drawn} optionFor={cardOption} startFor={cardStart} selected={selected} useStart={useStart} pointing={pointing} busy={busy} choose={choose} dragCard={o => dragCard(o, ['hand'])} />
        </Zone>
      )}
      <Zone zone="discard" count={spirit.discard.length} wanted={spirit.discard.some(c => cardOption(c.id) || cardStart(c.id))} ignoreClick={ignoreClick}>
        <CardList cards={spirit.discard} optionFor={cardOption} startFor={cardStart} selected={selected} useStart={useStart} pointing={pointing} busy={busy} choose={choose} dragCard={o => dragCard(o, ['hand'])} />
      </Zone>
      {info && info.specialRules.length > 0 && (
        <dialog class="rules-dialog" ref={rulesDialog} onClick={e => { if (e.target === rulesDialog.current) rulesDialog.current?.close(); }}>
          <h2>{spirit.name}: special rules</h2>
          {info.specialRules.map(r => <p key={r.title}><strong>{r.title}.</strong> {r.description}</p>)}
          <form method="dialog"><button type="submit">Close</button></form>
        </dialog>
      )}
      {carrying && (
        <img class="presence-carried" src={ownPresencePicture ?? PRESENCE_IMAGE} alt="" width={45} height={45} draggable={false}
          ref={el => { carriedPicture.current = el; placeCarried(); }}
          data-hue={ownPresencePicture ? undefined : presenceHue(presenceTokenImage)} />
      )}
    </section>
  );
}

interface TrackProps {
  title: ComponentChildren;
  track: TrackName;
  slots: readonly SlotView[];
  /** Indexes of the slots whose presence has been taken: those are the ones whose benefit you now have. */
  revealed: readonly number[];
  optionFor(track: TrackName, slot: number): OptionView | undefined;
  spirit: string;
  moves: ReadonlyMap<SourceKey, readonly OptionView[]>;
  selected: SourceKey | null;
  pick(source: SourceKey): () => void;
  /** Starts dragging the presence on this start (a press on it). */
  drag(source: SourceKey): (e: PointerEvent) => void;
  busy: boolean;
  choose(o: OptionView | undefined): () => void;
}

function Track({ title, track, slots, revealed, optionFor, spirit, moves, selected, pick, drag, busy, choose }: TrackProps) {
  return (
    <div class="track">
      <span class="track-label">{title}</span>
      <ol class="slots">
        {slots.map((slot, i) => {
          const open = revealed.includes(i);
          const option = optionFor(track, i);
          const source = slotSource(spirit, track, i);
          const movable = !option && moves.has(source); // presence that can be moved from this slot to a land
          // The presence sits on its slot's picture, a little above it; once taken, the slot is bare. A slot that presence can be put
          // back on shows a faint presence to click. Clicking the presence (not the slot) is how a slot is chosen.
          const classes = `slot-token ${open ? 'gone' : ''} ${option ? 'choosable' : ''} ${movable ? 'pickable' : ''} ${selected === source ? 'selected' : ''}`;
          const clickable = option ? choose(option) : movable ? pick(source) : null;
          const tokenImage = `Presence:${spirit}`;
          const own = presenceImageUrl(tokenImage);
          const picture = (
            <img src={own ?? PRESENCE_IMAGE} alt="presence" width={40} height={40} draggable={false}
              style={own ? undefined : { filter: `hue-rotate(${presenceHue(tokenImage)}deg)` }} />
          );
          return (
            <li key={i} class={`slot ${open ? 'revealed' : 'covered'}`}>
              <img class="slot-icon" src={trackImageUrl(slot.id)} alt={slot.label} title={slot.label} width={40} height={53} loading="lazy" decoding="async" />
              {clickable
                ? <button type="button" class={classes} disabled={busy} onClick={clickable} onPointerDown={!option && movable ? drag(source) : undefined} title={option ? slot.label : 'Pick up this presence, then choose a land'}>{picture}</button>
                : !open && <span class={classes}>{picture}</span>}
            </li>
          );
        })}
      </ol>
    </div>
  );
}

interface ZoneProps {
  zone: string;
  count: number;
  /** The decision is about one of its cards: it opens (the player can still close it, and open it again). */
  wanted: boolean;
  startOpen?: boolean;
  /** True for the click that ends a drag, which is not a request to fold or open. */
  ignoreClick(): boolean;
  children: ComponentChildren;
}

/**
 * A card area, always shown at full width with its name behind it, so a card can be dropped on it. Clicking its empty part (not a card)
 * folds or opens it; an area with no cards stays folded and does nothing. Folded, it shows only how many cards it holds.
 */
function Zone({ zone, count, wanted, startOpen = false, ignoreClick, children }: ZoneProps) {
  const [opened, setOpened] = useState(startOpen);
  const element = useRef<HTMLDivElement | null>(null);
  useEffect(() => { if (wanted) setOpened(true); }, [wanted]);
  useEffect(() => { // a card dropped on the area opens it
    const el = element.current;
    const open = () => setOpened(true);
    el?.addEventListener('zone-open', open);
    return () => el?.removeEventListener('zone-open', open);
  }, []);
  const open = 0 < count && opened;
  const toggle = (e: MouseEvent) => {
    if (0 < count && !ignoreClick() && !(e.target as HTMLElement).closest('button')) setOpened(!opened);
  };
  return (
    <div class={`zone zone-${zone} ${open ? 'open' : 'folded'} ${0 < count ? 'has-cards' : ''}`} data-drop-zone={zone} onClick={toggle} ref={element}>
      {!open && 0 < count && <span class="zone-count" title={`${count} card${count === 1 ? '' : 's'}; click to open`}>{count}</span>}
      {open && children}
    </div>
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
  cards: readonly CardView[];
  optionFor(card: string): OptionView | undefined;
  startFor(card: string): SourceKey | undefined;
  busy: boolean;
  choose(o: OptionView | undefined): () => void;
  /** Starts dragging a card that is chosen by clicking (not aimed): onto the area it goes to. */
  dragCard?(o: OptionView | undefined): ((e: PointerEvent) => void) | undefined;
}

function CardList({ cards, optionFor, startFor, selected, useStart, pointing, busy, choose, dragCard }: CardListProps) {
  return (
    <div>
      <ul class="cards">
        {cards.map(c => {
          const option = optionFor(c.id);
          const start = startFor(c.id);
          return (
            <li key={c.id}>
              <button
                type="button" class={`${option || start ? 'choosable' : ''} ${start && selected === start ? 'selected' : ''}`} disabled={busy || !(option || start)}
                onClick={start ? useStart(start) : choose(option)} {...(start ? pointing(start) : { onPointerDown: dragCard?.(option) })}
                title={`${c.name}: range ${rangeText(c.range)} · ${c.target}. ${c.instructions}`}
              >
                <Coin value={c.cost} title="energy to play" speed={c.speed} /> {c.name} <span class="card-elements"><Elements text={c.elements} /></span>
              </button>
            </li>
          );
        })}
      </ul>
    </div>
  );
}
