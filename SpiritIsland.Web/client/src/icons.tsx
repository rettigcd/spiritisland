import type { ComponentChildren } from 'preact';

/**
 * Small pictures used inside text: an element, a speed, a terrain. The text the server sends ("1 sun 2 water", "Fast", "M")
 * is turned into pictures here, with the words kept as tooltips and alternative text.
 */

const ELEMENTS = ['sun', 'moon', 'fire', 'air', 'water', 'plant', 'earth', 'animal'];
const ELEMENT_WORDS = /(?:(\d+)\s+)?\b(sun|moon|fire|air|water|plant|earth|animal|any)\b/gi;
const MOST_REPEATED = 4; // beyond this, one picture and a number reads better than a row of them

/** A picture in text. Its name is its alternative text; it only has a tooltip of its own if asked, because where it sits (a card, a tier) usually has the more useful one. */
function Icon({ name, title, tooltip = true }: { name: string; title: string; tooltip?: boolean }) {
  return <img class="icon" src={`/img/icons/${name}.png`} alt={title} title={tooltip ? title : undefined} width={18} height={18} loading="lazy" decoding="async" />;
}

/** Elements as pictures: "1 sun 2 water" is one sun and two waters (a count over four is a picture and a number). Other text stays text. */
export function Elements({ text }: { text: string }) {
  if (!text) return <span class="muted">none</span>;
  const parts: ComponentChildren[] = [];
  let last = 0;
  for (const m of text.matchAll(ELEMENT_WORDS)) {
    if (m.index > last) parts.push(text.slice(last, m.index));
    const name = (m[2] ?? '').toLowerCase();
    const count = m[1] ? Number(m[1]) : 1;
    const known = ELEMENTS.includes(name);
    parts.push(
      <span class="elements" key={m.index}>
        {!known ? m[0]
          : count <= MOST_REPEATED
            ? Array.from({ length: count }, (_, i) => <Icon key={i} name={name} title={name} tooltip={false} />)
            : <><Icon name={name} title={name} tooltip={false} /><span class="times">×{count}</span></>}
      </span>,
    );
    last = m.index + m[0].length;
  }
  if (last < text.length) parts.push(text.slice(last));
  return <span class="element-list">{parts}</span>;
}

/** Fast or slow as a picture with the word as its tooltip. Anything else stays text. */
export function Speed({ speed }: { speed: string }) {
  const lower = speed.toLowerCase();
  const fast = lower.includes('fast'), slow = lower.includes('slow');
  if (!fast && !slow) return <>{speed}</>;
  return (
    <span class="speed" title={fast && slow ? 'fast or slow' : fast ? 'fast' : 'slow'}>
      {fast && <Icon name="fast" title="fast" />}{slow && <Icon name="slow" title="slow" />}
    </span>
  );
}

/** An amount of energy: a number on top of a coin. For a card's cost, the speed picks the coin (red for fast, blue for slow), so the speed needs no icon of its own. */
export function Coin({ value, title = 'energy', speed }: { value: number; title?: string; speed?: string }) {
  const lower = speed?.toLowerCase() ?? '';
  const picture = lower.includes('fast') ? 'cost-fast' : lower.includes('slow') ? 'cost-slow' : 'coin';
  const tip = picture === 'coin' ? undefined : `${picture === 'cost-fast' ? 'fast' : 'slow'}, ${value} ${title}`;
  return (
    <span class="coin" title={tip}>
      <img src={`/img/icons/${picture}.png`} alt="" width={24} height={24} loading="lazy" decoding="async" />
      <span class="coin-value" aria-label={`${value} ${title}`}>{value}</span>
    </span>
  );
}

const TERRAINS: Record<string, { icon: string; name: string }> = {
  M: { icon: 'mountain', name: 'Mountain' },
  J: { icon: 'jungle', name: 'Jungle' },
  W: { icon: 'wetland', name: 'Wetland' },
  S: { icon: 'sands', name: 'Sands' },
  Coastal: { icon: 'coastal', name: 'Coastal' },
};

/**
 * An invader card as the terrains it names: "M", "J+S" or "Coastal", and "2M" for a stage 2 card that triggers escalation
 * (shown with the escalation mark). Any code that isn't recognised stays text.
 */
export function InvaderCard({ code }: { code: string }) {
  const m = /^(2)?(M|J|W|S|Coastal)(?:\+(M|J|W|S))?$/.exec(code);
  if (!m) return <>{code}</>;
  const terrains = [m[2], m[3]].flatMap(code => (code && TERRAINS[code]) || []);
  return (
    <span class="invader-card" title={`${terrains.map(t => t.name).join(' + ')}${m[1] ? ' (escalation)' : ''}`}>
      {m[1] && <Icon name="escalation" title="escalation" />}
      {terrains.map((t, i) => <Icon key={i} name={t.icon} title={t.name} />)}
    </span>
  );
}
