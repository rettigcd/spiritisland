export interface Point { x: number; y: number }

/**
 * The corners of a polygon pulled in so that every edge is `distance` inside the original (the same thing as the desktop app's
 * Polygons.InnerPoints, which it uses to draw a land's highlight inside the land and not on its borders). Works for either winding.
 * Each new corner is the old one moved along the bisector of its two edges' inward normals; a very sharp corner is limited so it
 * does not shoot far out.
 */
export function insetPolygon(corners: readonly Point[], distance: number): Point[] {
  const n = corners.length;
  if (n < 3) return [...corners];
  // The shoelace sign says which side of an edge is inside: positive, the left normal (-dy, dx) points in; negative, the other way.
  let area = 0;
  for (let i = 0; i < n; i++) {
    const a = corners[i]!, b = corners[(i + 1) % n]!;
    area += a.x * b.y - b.x * a.y;
  }
  const side = area >= 0 ? 1 : -1;
  const normal = (from: Point, to: Point): Point => {
    const dx = to.x - from.x, dy = to.y - from.y;
    const length = Math.hypot(dx, dy) || 1;
    return { x: (-dy / length) * side, y: (dx / length) * side };
  };
  return corners.map((corner, i) => {
    const prev = corners[(i + n - 1) % n]!, next = corners[(i + 1) % n]!;
    const n1 = normal(prev, corner), n2 = normal(corner, next);
    const scale = distance / Math.max(1 + n1.x * n2.x + n1.y * n2.y, 0.25);
    return { x: corner.x + (n1.x + n2.x) * scale, y: corner.y + (n1.y + n2.y) * scale };
  });
}
