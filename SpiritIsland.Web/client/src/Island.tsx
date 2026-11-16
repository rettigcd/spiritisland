import type { GameView, OptionView, SpaceGeometry, StaticView, TokenView } from './api/model';
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
  // Shrink long labels ("Blight", "Beast") so they stay inside the circle.
  const size = Math.min(space.tokenSize * 0.4, (space.tokenSize * 1.5) / Math.max(token.label.length, 1));
  return (
    <g class={['token', option ? 'choosable' : ''].join(' ')} onClick={option ? () => choose(option) : undefined}>
      <title>{`${token.label}${token.count > 1 ? ` x${token.count}` : ''}${token.strife > 0 ? ` (strife ${token.strife})` : ''}`}</title>
      <circle cx={token.x} cy={token.y} r={r} fill={colourOf(token.image, 72)} stroke={token.sacredSite ? '#a06000' : '#444'} stroke-width={token.sacredSite ? r * 0.2 : r * 0.08} />
      <text x={token.x} y={token.y} font-size={size} text-anchor="middle" dominant-baseline="central">{token.label}</text>
      {token.count > 1 && <text class="count" x={token.x + r} y={token.y - r} font-size={size} text-anchor="middle" dominant-baseline="central">{token.count}</text>}
    </g>
  );
}
