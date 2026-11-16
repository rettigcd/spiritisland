/**
 * A card's or innate power's range as the engine prints it, in words:
 * "1" is range 1 from presence, "2:ss" is range 2 from a sacred site, "1:Mountain" is range 1 from presence in a Mountain,
 * "+1" is extra range, and "-" is no range limit (or no range at all, for a power that targets a spirit).
 */
export function rangeText(range: string): string {
  if (range === '-') return 'any distance';
  const from = /^(\d+):(.+)$/.exec(range);
  if (!from) return range;
  const [, distance, origin] = from;
  return origin === 'ss' ? `${distance} from a sacred site` : `${distance} from presence in ${origin}`;
}
