/**
 * The shared access token. The server wants it in a header on every API call; it is kept in localStorage so the player types it
 * once per browser. When the server turns a call down (no token, or the wrong one) the app shows the prompt page.
 */

export const ACCESS_HEADER = 'X-Access-Token';
const KEY = 'accessToken';

export function savedToken(): string | null {
  try { return localStorage.getItem(KEY) || null; } catch { return null; }
}

export function saveToken(token: string): void {
  try { localStorage.setItem(KEY, token); } catch { /* the token just has to be typed again next time */ }
}

export function forgetToken(): void {
  try { localStorage.removeItem(KEY); } catch { /* nothing to forget */ }
}

type Listener = (rejected: boolean) => void;
let listener: Listener | null = null;

/** Hears when the server turns a call down for want of a valid token; `rejected` is true if a saved token was sent and was wrong. */
export function onAccessNeeded(l: Listener): () => void {
  listener = l;
  return () => { if (listener === l) listener = null; };
}

export function accessNeeded(rejected: boolean): void {
  listener?.(rejected);
}
