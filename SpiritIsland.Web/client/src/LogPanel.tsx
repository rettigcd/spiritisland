import { useEffect, useRef } from 'preact/hooks';
import type { LogLineView } from './api/model';

/**
 * What has happened in the game, oldest first, in a dialog laid over the game (opened from the menu, closed with the button,
 * Escape or a click outside it), kept scrolled to the newest line. The server decides what is shown (round and phase markers,
 * what the invaders did, blight, fear, presence destroyed, the end of the game) and sends the most recent lines. A line's kind
 * only picks its colour.
 */
export function LogPanel({ log, onClose }: { log: readonly LogLineView[]; onClose(): void }) {
  const dialog = useRef<HTMLDialogElement>(null);
  const box = useRef<HTMLOListElement>(null);

  // The browser's own modal dialog: it blocks the page behind it and handles Escape and focus.
  useEffect(() => {
    const el = dialog.current;
    if (el && !el.open) el.showModal();
  }, []);

  // Show the newest line whenever the log grows (and when it opens).
  useEffect(() => {
    const el = box.current;
    if (el) el.scrollTop = el.scrollHeight;
  }, [log.length]);

  return (
    <dialog
      ref={dialog} class="log" aria-label="Game log"
      onClose={onClose}
      onClick={e => { if (e.target === dialog.current) onClose(); }} // a click on the dimmed area outside the box
    >
      <div class="log-head">
        <h2>Log</h2>
        <button type="button" onClick={onClose}>Close</button>
      </div>
      <ol ref={box} class="log-lines">
        {log.map((line, i) => (
          <li key={i} class={`log-${line.kind}`}>{line.text}</li>
        ))}
      </ol>
    </dialog>
  );
}
