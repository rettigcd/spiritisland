import type { GameView, OptionView, SpaceGeometry, StaticView, TokenView } from './api/model';
import { isPresence, PRESENCE_IMAGE, presenceHue, STRIFE_IMAGE, tokenImageUrl } from './images';
import { boardOptions, spacesOf, tokenKey } from './options';

interface Props {
  staticView: StaticView;
  view: GameView;
  options: readonly OptionView[];
  /** The option the player is pointing at in the decision panel, so the island can show where it applies. */
  hovered: OptionView | null;
  disabled: boolean;
  onChoose(optionId: string): void;
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

export function Island({ staticView, view, options, hovered, disabled, onChoose }: Props) {
  const { x, y, width, height } = staticView.worldBounds;
  const { bySpace, byToken } = boardOptions(options);
  const hoveredSpaces = new Set(spacesOf(hovered));
  const tokensBySpace = new Map(view.spaces.map(s => [s.id, s.tokens]));

  const choose = (o: OptionView) => { if (!disabled) onChoose(o.id); };

  return (
    <svg class="island" viewBox={`${x} ${y} ${width} ${height}`} role="img" aria-label="The island">
      {staticView.spaces.map(space => {
        const option = bySpace.get(space.id);
        return (
          <g key={space.id}>
            <polygon
              points={space.corners.map(c => `${c.x},${c.y}`).join(' ')}
              fill={fillOf(space.terrain)}
              class={['space', option ? 'choosable' : '', hoveredSpaces.has(space.id) ? 'hovered' : ''].join(' ')}
              onClick={option ? () => choose(option) : undefined}
            >
              <title>{option ? `${space.id} (${space.terrain}) - choose` : `${space.id} (${space.terrain})`}</title>
            </polygon>
            <text class="space-name" x={space.nameLocation.x} y={space.nameLocation.y} font-size={space.tokenSize * 0.45}>{space.id}</text>
            {(tokensBySpace.get(space.id) ?? []).map(t => (
              <Token key={t.id} token={t} space={space} option={byToken.get(tokenKey(space.id, t.id))} choose={choose} />
            ))}
          </g>
        );
      })}
    </svg>
  );
}

interface TokenProps {
  token: TokenView;
  space: SpaceGeometry;
  option: OptionView | undefined;
  choose(o: OptionView): void;
}

function Token({ token, space, option, choose }: TokenProps) {
  const r = space.tokenSize / 2;
  const url = isPresence(token.image) ? PRESENCE_IMAGE : tokenImageUrl(token.image);
  const small = space.tokenSize * 0.3; // the size of the badges and counts
  const title = `${token.label}${token.count > 1 ? ` x${token.count}` : ''}${token.strife > 0 ? ` (strife ${token.strife})` : ''}`;
  return (
    <g class={['token', option ? 'choosable' : ''].join(' ')} onClick={option ? () => choose(option) : undefined}>
      <title>{title}</title>
      {url ? <Picture token={token} url={url} r={r} small={small} /> : <Circle token={token} r={r} />}
      {0 < token.strife && url && (
        <image href={STRIFE_IMAGE} x={token.x - r} y={token.y - r} width={r * 0.9} height={r * 0.9} />
      )}
      {token.badge && url && <Badge x={token.x + r * 0.55} y={token.y + r * 0.7} text={token.badge} size={small} />}
      {token.count > 1 && <Badge x={token.x + r * 0.75} y={token.y - r * 0.75} text={String(token.count)} size={small} bold />}
    </g>
  );
}

/** A token's picture, with a ring for a sacred site and (when it is an option) a ring to click. */
function Picture({ token, url, r, small }: { token: TokenView; url: string; r: number; small: number }) {
  const presence = isPresence(token.image);
  return (
    <>
      {token.sacredSite && <circle class="ring-sacred" cx={token.x} cy={token.y} r={r * 1.05} fill="none" stroke="#a06000" stroke-width={r * 0.18} />}
      <circle class="ring-choose" cx={token.x} cy={token.y} r={r * 1.1} fill="none" />
      <image
        href={url} x={token.x - r} y={token.y - r} width={r * 2} height={r * 2}
        style={presence ? { filter: `hue-rotate(${presenceHue(token.image)}deg)` } : undefined}
      />
      {presence && (
        <text class="presence-label" x={token.x} y={token.y} font-size={small * 0.9} stroke-width={small * 0.3} text-anchor="middle" dominant-baseline="central">{token.label}</text>
      )}
    </>
  );
}

/** What is drawn for a token that has no picture. */
function Circle({ token, r }: { token: TokenView; r: number }) {
  // Shrink long labels so they stay inside the circle.
  const size = Math.min(r * 0.8, (r * 3) / Math.max(token.label.length, 1));
  return (
    <>
      <circle cx={token.x} cy={token.y} r={r} fill={colourOf(token.image, 72)} stroke={token.sacredSite ? '#a06000' : '#444'} stroke-width={token.sacredSite ? r * 0.2 : r * 0.08} />
      <circle class="ring-choose" cx={token.x} cy={token.y} r={r * 1.1} fill="none" />
      <text x={token.x} y={token.y} font-size={size} text-anchor="middle" dominant-baseline="central">{token.label}</text>
    </>
  );
}

/** A small number or word on a token (its health, or how many are stacked). */
function Badge({ x, y, text, size, bold }: { x: number; y: number; text: string; size: number; bold?: boolean }) {
  const width = size * (0.7 + 0.6 * text.length);
  return (
    <>
      <rect class="badge" x={x - width / 2} y={y - size * 0.6} width={width} height={size * 1.2} rx={size * 0.3} />
      <text class={bold ? 'badge-text bold' : 'badge-text'} x={x} y={y} font-size={size} text-anchor="middle" dominant-baseline="central">{text}</text>
    </>
  );
}
