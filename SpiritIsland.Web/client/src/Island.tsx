import { useEffect, useLayoutEffect, useRef, useState } from 'preact/hooks';
import type { GameView, OptionView, SpaceGeometry, StaticView, TokenView } from './api/model';
import { isPresence, PRESENCE_IMAGE, presenceHue, presenceImageUrl, STRIFE_IMAGE, tokenImageUrl } from './images';
import { destinationsOf, tokenSource } from './moves';
import type { SourceKey } from './moves';
import { boardOptions, spacesOf, tokenKey } from './options';

interface Props {
  staticView: StaticView;
  view: GameView;
  options: readonly OptionView[];
  /** The option the player is pointing at in the decision panel, so the island can show where it applies. */
  hovered: OptionView | null;
  /** The action the pointer is on (a card, an innate power): the lands it could target are shown. */
  hoveredSource: SourceKey | null;
  /** The decision's moves by where they start, and the start the player has picked (if any). */
  moves: ReadonlyMap<SourceKey, readonly OptionView[]>;
  selected: SourceKey | null;
  onSelect(source: SourceKey | null): void;
  disabled: boolean;
  onChoose(optionId: string): void;
}

/** A token being dragged: where it started, whether it has really moved yet (a click is not a drag), and where the pointer is. */
interface Drag {
  source: SourceKey;
  token: TokenView;
  space: SpaceGeometry;
  wasSelected: boolean;
  startX: number;
  startY: number;
  moved: boolean;
  at: { x: number; y: number };
}

/** A stable colour per name: good enough to tell tokens apart until there are pictures. */
function colourOf(name: string, lightness: number): string {
  let h = 0;
  for (const ch of name) h = (h * 31 + ch.charCodeAt(0)) >>> 0;
  return `hsl(${h % 360} 55% ${lightness}%)`;
}

const terrainFill: [RegExp, string][] = [
  [/ocean/i, '#9ec9e8'],
  [/jungle/i, '#9fd39a'],
  [/wetland/i, '#9ad6cf'],
  [/sands/i, '#ecd89a'],
  [/mountain/i, '#c4b7a8'],
];
const fillOf = (terrain: string) => terrainFill.find(([re]) => re.test(terrain))?.[1] ?? '#dddddd';

/**
 * How many of the island's own units one screen pixel is. The island is drawn in the server's units (about 1.5 wide), so an
 * outline that should be a few pixels thick is a few of these. (CSS's `vector-effect: non-scaling-stroke` would do this, but on
 * some GPUs it draws black boxes, so the widths are worked out here and set on the shapes.)
 */
function useUnitsPerPixel(svg: { current: SVGSVGElement | null }, worldWidth: number): number {
  const [unit, setUnit] = useState(worldWidth / 800);
  useLayoutEffect(() => {
    const el = svg.current;
    if (!el) return;
    const measure = () => { if (el.clientWidth > 0) setUnit(worldWidth / el.clientWidth); };
    measure();
    const observer = new ResizeObserver(measure);
    observer.observe(el);
    return () => observer.disconnect();
  }, [worldWidth]);
  return unit;
}

export function Island({ staticView, view, options, hovered, hoveredSource, moves, selected, onSelect, disabled, onChoose }: Props) {
  const { x, y, width, height } = staticView.worldBounds;
  const { bySpace, byToken } = boardOptions(options);
  const tokensBySpace = new Map(view.spaces.map(s => [s.id, s.tokens]));
  const svg = useRef<SVGSVGElement>(null);
  const pixel = useUnitsPerPixel(svg, width);
  const [drag, setDrag] = useState<Drag | null>(null);
  const [over, setOver] = useState<string | null>(null); // the land the pointer is over while a move is being made

  // Once a start is picked, every land its moves can reach is known. Pointing at an action (a card, an innate power) shows
  // the lands it could target as well, without picking it.
  const reachFrom = selected ?? hoveredSource;
  const reach = reachFrom ? destinationsOf(moves.get(reachFrom) ?? []) : new Map<string, OptionView>();
  const destinations = selected ? reach : new Map<string, OptionView>(); // only a picked start can be clicked through to a land
  const overMove = over ? destinations.get(over) : undefined;
  const shown = overMove ?? hovered; // what the island shows the effect of: the land the pointer is over, or the hovered button
  const hoveredSpaces = new Set(spacesOf(shown));
  const arrow = moveArrow(shown, staticView, view);

  const choose = (o: OptionView) => { if (!disabled) onChoose(o.id); };

  // ---- dragging a token: press on it, move, release on a land it can reach. A press and release without moving is a click,
  // ---- which leaves it picked (or puts it down if it already was), so the destination can be clicked instead.
  const toSvg = (e: PointerEvent) => {
    const el = svg.current!;
    const point = el.createSVGPoint();
    point.x = e.clientX;
    point.y = e.clientY;
    const matrix = el.getScreenCTM();
    const at = matrix ? point.matrixTransform(matrix.inverse()) : point;
    return { x: at.x, y: at.y };
  };
  const landUnder = (e: PointerEvent) =>
    document.elementFromPoint(e.clientX, e.clientY)?.closest('[data-space]')?.getAttribute('data-space') ?? null;

  const beginDrag = (e: PointerEvent, source: SourceKey, token: TokenView, space: SpaceGeometry) => {
    if (disabled) return;
    e.preventDefault();
    (e.currentTarget as Element).setPointerCapture?.(e.pointerId); // keep hearing about the pointer when it leaves the token
    setDrag({ source, token, space, wasSelected: selected === source, startX: e.clientX, startY: e.clientY, moved: false, at: toSvg(e) });
    onSelect(source);
  };
  // The pointer can report several times a frame; only the newest position is drawn, once per frame.
  const frame = useRef(0);
  const latest = useRef<PointerEvent | null>(null);
  useEffect(() => () => cancelAnimationFrame(frame.current), []);
  const dragTo = (e: PointerEvent) => {
    if (!drag) return;
    latest.current = e;
    if (frame.current) return;
    frame.current = requestAnimationFrame(() => {
      frame.current = 0;
      const last = latest.current;
      if (!last) return;
      setDrag(d => {
        if (!d) return d;
        const moved = d.moved || Math.hypot(last.clientX - d.startX, last.clientY - d.startY) > 5;
        return { ...d, moved, at: toSvg(last) };
      });
      setOver(landUnder(last));
    });
  };
  const endDrag = (e: PointerEvent) => {
    if (!drag) return;
    cancelAnimationFrame(frame.current);
    frame.current = 0;
    latest.current = null;
    const finished = drag;
    setDrag(null);
    setOver(null);
    // (judged from where the pointer is released: a quick flick can end before the first frame has marked the drag as moved)
    if (finished.moved || Math.hypot(e.clientX - finished.startX, e.clientY - finished.startY) > 5) {
      const land = landUnder(e);
      const move = land ? destinations.get(land) : undefined;
      if (move) choose(move);
      else onSelect(null); // dropped somewhere it cannot go: put it down
    } else if (finished.wasSelected) onSelect(null); // clicked a token that was already picked: put it down
  };

  return (
    <svg
      ref={svg} class={['island', drag?.moved ? 'dragging' : ''].join(' ')} viewBox={`${x} ${y} ${width} ${height}`} role="img" aria-label="The island"
      onPointerMove={dragTo} onPointerUp={endDrag} onPointerCancel={() => { setDrag(null); setOver(null); }}
    >
      <defs>
        {/* The glow behind presence on a sacred site: bright gold in the middle, fading to nothing at the edge. */}
        <radialGradient id="sacred-glow">
          <stop offset="0%" stop-color="#ffd23f" stop-opacity="0.95" />
          <stop offset="55%" stop-color="#ffc400" stop-opacity="0.75" />
          <stop offset="100%" stop-color="#ffc400" stop-opacity="0" />
        </radialGradient>
      </defs>
      {staticView.spaces.map(space => {
        const option = bySpace.get(space.id);
        const destination = destinations.get(space.id);
        const reachable = reach.has(space.id);
        const onClick = option ? () => choose(option) : destination ? () => choose(destination) : selected ? () => onSelect(null) : undefined;
        return (
          <g key={space.id} data-space={space.id}>
            <polygon
              points={space.corners.map(c => `${c.x},${c.y}`).join(' ')}
              fill={fillOf(space.terrain)}
              stroke-width={pixel * (reachable || option ? 4 : hoveredSpaces.has(space.id) ? 5 : 1.5)}
              class={['space', option ? 'choosable' : '', reachable ? 'destination' : '', hoveredSpaces.has(space.id) ? 'hovered' : ''].join(' ')}
              onClick={onClick}
              onPointerEnter={selected ? () => setOver(space.id) : undefined}
              onPointerLeave={selected && !drag ? () => setOver(null) : undefined}
            >
              {!selected && <title>{option ? `${space.id} (${space.terrain}) - choose` : `${space.id} (${space.terrain})`}</title>}
            </polygon>
            <text class="space-name" x={space.nameLocation.x} y={space.nameLocation.y} font-size={space.tokenSize * 0.45}>{space.id}</text>
            {(tokensBySpace.get(space.id) ?? []).map(t => {
              const source = tokenSource(space.id, t.id);
              const chooses = byToken.get(tokenKey(space.id, t.id));
              const movable = !chooses && moves.has(source);
              return (
                <Token
                  key={t.id} token={t} space={space} option={chooses} choose={choose}
                  movable={movable} selected={selected === source} quiet={selected !== null}
                  onPointerDown={movable ? e => beginDrag(e, source, t, space) : undefined}
                />
              );
            })}
          </g>
        );
      })}
      {arrow && <Arrow from={arrow.from} to={arrow.to} size={arrow.size} width={pixel * 4} />}
      {drag?.moved && (
        <g class="ghost">
          <Token token={{ ...drag.token, x: drag.at.x, y: drag.at.y }} space={drag.space} option={undefined} choose={choose} movable={false} selected={false} quiet flat />
        </g>
      )}
    </svg>
  );
}

interface Point { x: number; y: number }

const centre = (space: SpaceGeometry): Point => ({
  x: space.corners.reduce((sum, c) => sum + c.x, 0) / space.corners.length,
  y: space.corners.reduce((sum, c) => sum + c.y, 0) / space.corners.length,
});

/**
 * Where a hovered move would go, as an arrow: from the token (or the middle of its land) to the middle of the destination.
 * A move that does not start and end on the island (presence from a track, or back from the destroyed pile) has none.
 */
function moveArrow(option: OptionView | null, staticView: StaticView, view: GameView): { from: Point; to: Point; size: number } | null {
  const target = option?.target;
  if (!target || target.kind !== 'move' || !target.space || !target.toSpace) return null;
  const from = staticView.spaces.find(s => s.id === target.space);
  const to = staticView.spaces.find(s => s.id === target.toSpace);
  if (!from || !to || from.id === to.id) return null;
  const token = view.spaces.find(s => s.id === from.id)?.tokens.find(t => t.id === target.token);
  return { from: token ? { x: token.x, y: token.y } : centre(from), to: centre(to), size: from.tokenSize };
}

/** A line with a head, drawn over the island. Sized from the token size, because the island is drawn in its own small units. */
function Arrow({ from, to, size, width }: { from: Point; to: Point; size: number; width: number }) {
  const dx = to.x - from.x, dy = to.y - from.y;
  const length = Math.hypot(dx, dy);
  if (length === 0) return null;
  const ux = dx / length, uy = dy / length; // along the arrow
  const head = size * 0.7;
  const base = { x: to.x - ux * head, y: to.y - uy * head };
  const points = [
    `${to.x},${to.y}`,
    `${base.x - uy * head * 0.45},${base.y + ux * head * 0.45}`,
    `${base.x + uy * head * 0.45},${base.y - ux * head * 0.45}`,
  ].join(' ');
  return (
    <g class="arrow">
      <line x1={from.x} y1={from.y} x2={base.x} y2={base.y} stroke-width={width} />
      <polygon points={points} />
    </g>
  );
}

interface TokenProps {
  token: TokenView;
  space: SpaceGeometry;
  /** Set when clicking the token chooses an option. */
  option: OptionView | undefined;
  choose(o: OptionView): void;
  /** Set when the token is where a move can start: it can be picked up (clicked, or dragged to a land). */
  movable: boolean;
  selected: boolean;
  /** Draw no tooltip: while a token is picked up, tooltips would come and go as the pointer crosses lands. */
  quiet?: boolean;
  /** The copy that follows a drag: the picture if it is plain, but no colour filter (presence is a plain disc in its colour). */
  flat?: boolean;
  onPointerDown?: (e: PointerEvent) => void;
}

function Token({ token, space, option, choose, movable, selected, quiet, flat, onPointerDown }: TokenProps) {
  const r = space.tokenSize / 2;
  const url = isPresence(token.image) ? (presenceImageUrl(token.image) ?? (flat ? null : PRESENCE_IMAGE)) : tokenImageUrl(token.image);
  const small = space.tokenSize * 0.3; // the size of the badges and counts
  const title = `${token.label}${token.count > 1 ? ` x${token.count}` : ''}${token.strife > 0 ? ` (strife ${token.strife})` : ''}`;
  return (
    <g
      class={['token', option ? 'choosable' : '', movable ? 'movable' : '', selected ? 'selected' : ''].join(' ')}
      onClick={option ? () => choose(option) : undefined} onPointerDown={onPointerDown}
    >
      {!quiet && <title>{title}</title>}
      {url ? <Picture token={token} url={url} r={r} small={small} /> : <Circle token={token} r={r} />}
      {0 < token.strife && url && (
        <image href={STRIFE_IMAGE} x={token.x - r} y={token.y - r} width={r * 0.9} height={r * 0.9} />
      )}
      {token.badge && url && <Badge x={token.x + r * 0.55} y={token.y + r * 0.7} text={token.badge} size={small} health />}
      {token.count > 1 && <Badge x={token.x + r * 0.75} y={token.y - r * 0.75} text={`x${token.count}`} size={small} />}
    </g>
  );
}

/** A token's picture, with a glow behind it for a sacred site and (when it is an option) a ring to click. */
function Picture({ token, url, r, small }: { token: TokenView; url: string; r: number; small: number }) {
  const presence = isPresence(token.image);
  return (
    <>
      {token.sacredSite && <Glow x={token.x} y={token.y} r={r} />}
      <circle class="ring-choose" cx={token.x} cy={token.y} r={r * 1.1} fill="none" stroke-width={r * 0.2} />
      <circle class="ring-move" cx={token.x} cy={token.y} r={r * 1.1} fill="none" stroke-width={r * 0.18} stroke-dasharray={`${r * 0.5} ${r * 0.35}`} />
      <image
        href={url} x={token.x - r} y={token.y - r} width={r * 2} height={r * 2}
        style={presence && !presenceImageUrl(token.image) ? { filter: `hue-rotate(${presenceHue(token.image)}deg)` } : undefined}
      />
      {presence && (
        <text class="presence-label" x={token.x} y={token.y} font-size={small * 0.9} stroke-width={small * 0.3} text-anchor="middle" dominant-baseline="central">{token.label}</text>
      )}
    </>
  );
}

/** The glow of a sacred site: a soft golden disc, bigger than the token, behind it. */
function Glow({ x, y, r }: { x: number; y: number; r: number }) {
  return <circle class="sacred-glow" cx={x} cy={y} r={r * 1.8} fill="url(#sacred-glow)" pointer-events="none" />;
}

/** What is drawn for a token that has no picture. */
function Circle({ token, r }: { token: TokenView; r: number }) {
  // Shrink long labels so they stay inside the circle.
  const size = Math.min(r * 0.8, (r * 3) / Math.max(token.label.length, 1));
  const fill = isPresence(token.image) ? `hsl(${presenceHue(token.image)} 85% 30%)` : colourOf(token.image, 72);
  return (
    <>
      {token.sacredSite && <Glow x={token.x} y={token.y} r={r} />}
      <circle cx={token.x} cy={token.y} r={r} fill={fill} stroke="#444" stroke-width={r * 0.08} />
      <circle class="ring-choose" cx={token.x} cy={token.y} r={r * 1.1} fill="none" stroke-width={r * 0.2} />
      <circle class="ring-move" cx={token.x} cy={token.y} r={r * 1.1} fill="none" stroke-width={r * 0.18} stroke-dasharray={`${r * 0.5} ${r * 0.35}`} />
      <text x={token.x} y={token.y} font-size={size} text-anchor="middle" dominant-baseline="central">{token.label}</text>
    </>
  );
}

/** A small number on a token (its health, or how many are stacked): outlined text, with no box, like the numbers on the energy coins. */
function Badge({ x, y, text, size, health }: { x: number; y: number; text: string; size: number; health?: boolean }) {
  return (
    <text
      class={health ? 'badge-text health' : 'badge-text'} x={x} y={y} font-size={size * 1.15} stroke-width={size * 0.4}
      text-anchor="middle" dominant-baseline="central"
    >{text}</text>
  );
}
