import type { GameSummary } from './api/model';

/**
 * What this browser has played, kept in localStorage: the games (id -> when last played) and the last few spirits.
 * The server keeps every game forever and has no idea who played what, so the Saved games list is the server's list
 * narrowed to the games played here in the last month.
 */
const PLAYED = 'si:played';
const SPIRITS = 'si:recent-spirits';
const GAME_PREFIX = 'si:game:';
const MONTH_MS = 30 * 24 * 60 * 60 * 1000;
export const RECENT_SPIRITS = 4;

function read<T>(key: string, fallback: T): T {
  try { const text = localStorage.getItem(key); return text ? JSON.parse(text) as T : fallback; } catch { return fallback; }
}
function write(key: string, value: unknown) {
  try { localStorage.setItem(key, JSON.stringify(value)); } catch { /* not kept: the list just starts over */ }
}

/** Notes that this game was played just now. */
export function markPlayed(gameId: string) {
  const played = read<Record<string, number>>(PLAYED, {});
  played[gameId] = Date.now();
  write(PLAYED, played);
}

/**
 * The games to list: those played here within the last month, newest first. Games older than that are forgotten here (their saved
 * picks too; the server still has them). A game this browser saved picks for before the list existed counts as played when the
 * server last saw it.
 */
export function gamesPlayedHere(all: readonly GameSummary[]): GameSummary[] {
  const played = read<Record<string, number>>(PLAYED, {});
  const cutoff = Date.now() - MONTH_MS;
  const when = (g: GameSummary) => Math.max(played[g.gameId] ?? 0, Date.parse(g.lastPlayed) || 0);
  for (const g of all) {
    let saved = false;
    try { saved = localStorage.getItem(GAME_PREFIX + g.gameId) !== null; } catch { /* treated as not saved */ }
    if (saved && played[g.gameId] === undefined) played[g.gameId] = Date.parse(g.lastPlayed) || Date.now();
  }
  for (const id of Object.keys(played))
    if ((played[id] ?? 0) < cutoff) {
      delete played[id];
      try { localStorage.removeItem(GAME_PREFIX + id); } catch { /* ignored */ }
    }
  write(PLAYED, played);
  return all.filter(g => played[g.gameId] !== undefined).sort((a, b) => when(b) - when(a));
}

/** The spirits played last, newest first, each once. */
export function recentSpirits(): string[] {
  const spirits = read<unknown>(SPIRITS, []);
  return Array.isArray(spirits) ? spirits.filter((s): s is string => typeof s === 'string').slice(0, RECENT_SPIRITS) : [];
}

export function noteSpirit(spirit: string) {
  write(SPIRITS, [spirit, ...recentSpirits().filter(s => s !== spirit)].slice(0, RECENT_SPIRITS));
}
