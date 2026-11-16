import { useEffect, useRef, useState } from 'preact/hooks';

/** A hamburger button that opens a list of buttons. It closes when something is chosen, on Escape, or on a click elsewhere. */
export function Menu({ items }: { items: readonly { label: string; title?: string; disabled?: boolean; onSelect(): void }[] }) {
  const [open, setOpen] = useState(false);
  const root = useRef<HTMLDivElement>(null);

  useEffect(() => {
    if (!open) return;
    const onDown = (e: MouseEvent) => { if (!root.current?.contains(e.target as Node)) setOpen(false); };
    const onKey = (e: KeyboardEvent) => { if (e.key === 'Escape') setOpen(false); };
    document.addEventListener('mousedown', onDown);
    document.addEventListener('keydown', onKey);
    return () => { document.removeEventListener('mousedown', onDown); document.removeEventListener('keydown', onKey); };
  }, [open]);

  return (
    <div class="menu" ref={root}>
      <button type="button" class="menu-button" aria-haspopup="menu" aria-expanded={open} aria-label="Menu" title="Menu" onClick={() => setOpen(o => !o)}>☰</button>
      {open && (
        <ul class="menu-list" role="menu">
          {items.map(item => (
            <li key={item.label} role="none">
              <button type="button" role="menuitem" title={item.title} disabled={item.disabled} onClick={() => { setOpen(false); item.onSelect(); }}>{item.label}</button>
            </li>
          ))}
        </ul>
      )}
    </div>
  );
}
