import type { ComponentChildren } from 'preact';
import { useEffect, useState } from 'preact/hooks';

/**
 * First-time hints: a short note shown until the player has done the thing it teaches, then never again. The ones already done are
 * kept in localStorage. A hint floats over the page (it takes no room and no clicks), so showing it moves nothing.
 */
const KEY = 'si:hints-done';
const listeners = new Set<() => void>();

function done(): string[] {
  try { const v = JSON.parse(localStorage.getItem(KEY) ?? '[]') as unknown; return Array.isArray(v) ? v.filter((s): s is string => typeof s === 'string') : []; } catch { return []; }
}

/** The player has done what the hint teaches: it is not shown again. */
export function hintDone(id: string) {
  if (done().includes(id)) return;
  try { localStorage.setItem(KEY, JSON.stringify([...done(), id])); } catch { /* not kept: it shows again next visit */ }
  listeners.forEach(l => l());
}

/** Shows its text until `hintDone(id)` is called (now or on an earlier visit). */
export function Hint({ id, children }: { id: string; children: ComponentChildren }) {
  const [shown, setShown] = useState(() => !done().includes(id));
  useEffect(() => {
    const check = () => setShown(!done().includes(id));
    listeners.add(check);
    return () => { listeners.delete(check); };
  }, [id]);
  return shown ? <div class="hint" role="note">{children}</div> : null;
}
