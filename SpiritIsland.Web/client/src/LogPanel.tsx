import { useEffect, useRef } from 'preact/hooks';
import type { LogLineView } from './api/model';

/**
 * What has happened in the game, oldest first, kept scrolled to the newest line. The server decides what is shown (round and
 * phase markers, what the invaders did, blight, fear, presence destroyed, the end of the game) and sends the most recent lines.
 * A line's kind only picks its colour.
 */
export function LogPanel({ log }: { log: readonly LogLineView[] }) {
  const box = useRef<HTMLOListElement>(null);

  // Show the newest line whenever the log grows.
  useEffect(() => {
    const el = box.current;
    if (el) el.scrollTop = el.scrollHeight;
  }, [log.length]);

  return (
    <section class="panel log" aria-label="Game log">
      <h2>Log</h2>
      <ol ref={box} class="log-lines">
        {log.map((line, i) => (
          <li key={i} class={`log-${line.kind}`}>{line.text}</li>
        ))}
      </ol>
    </section>
  );
}
