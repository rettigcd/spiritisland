import { useState } from 'preact/hooks';

const DEFAULT_WIDTH = 440, MIN_WIDTH = 240, MAX_WIDTH = 900, KEY = 'sideWidth';

const clamp = (w: number) => Math.min(MAX_WIDTH, Math.max(MIN_WIDTH, Math.round(w)));

/** The width of the right-hand column, remembered by the browser (if it lets us). */
export function useSideWidth(): [number, (width: number) => void] {
  const [width, setWidth] = useState(() => {
    try { const saved = Number(localStorage.getItem(KEY)); return saved ? clamp(saved) : DEFAULT_WIDTH; } catch { return DEFAULT_WIDTH; }
  });
  const set = (w: number) => {
    const next = clamp(w);
    setWidth(next);
    try { localStorage.setItem(KEY, String(next)); } catch { /* the width just isn't remembered */ }
  };
  return [width, set];
}

/**
 * The handle on the right-hand edge of the right-hand column: drag it right to widen the column, left to narrow it.
 * Arrow keys move it too, and a double click puts it back.
 */
export function Resizer({ width, onResize }: { width: number; onResize(width: number): void }) {
  const [drag, setDrag] = useState<{ startX: number; startWidth: number } | null>(null);
  return (
    <div
      class={`resizer ${drag ? 'resizing' : ''}`} role="separator" aria-orientation="vertical" aria-label="Resize the right column"
      aria-valuenow={width} aria-valuemin={MIN_WIDTH} aria-valuemax={MAX_WIDTH} tabIndex={0} title="Drag to resize (double-click to reset)"
      onPointerDown={e => { (e.currentTarget as Element).setPointerCapture(e.pointerId); setDrag({ startX: e.clientX, startWidth: width }); }}
      onPointerMove={e => { if (drag) onResize(drag.startWidth + e.clientX - drag.startX); }}
      onPointerUp={() => setDrag(null)}
      onPointerCancel={() => setDrag(null)}
      onDblClick={() => onResize(DEFAULT_WIDTH)}
      onKeyDown={e => {
        if (e.key === 'ArrowRight') { onResize(width + 20); e.preventDefault(); }
        else if (e.key === 'ArrowLeft') { onResize(width - 20); e.preventDefault(); }
      }}
    />
  );
}
