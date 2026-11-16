import type { OptionView } from './api/model';

/**
 * What the current decision's options point at on the island. An option's target says what it is (see OptionTarget on the
 * server); the island only needs the spaces and tokens, everything else is a button in the decision panel.
 */
export interface BoardOptions {
  /** Options that are a space: click the space. Keyed by space id. */
  bySpace: Map<string, OptionView>;
  /** Options that are a token on a space: click the token. Keyed by `${space}|${token}`. */
  byToken: Map<string, OptionView>;
}

export const tokenKey = (space: string, token: string) => `${space}|${token}`;

export function boardOptions(options: readonly OptionView[]): BoardOptions {
  const bySpace = new Map<string, OptionView>();
  const byToken = new Map<string, OptionView>();
  for (const o of options) {
    const t = o.target;
    if (t.kind === 'space' && t.space) bySpace.set(t.space, o);
    else if (t.kind === 'spaceToken' && t.space && t.token) byToken.set(tokenKey(t.space, t.token), o);
  }
  return { bySpace, byToken };
}

/** The spaces an option involves: where it is, and for a move, where it goes. Used to show what a button would do. */
export function spacesOf(option: OptionView | null): string[] {
  if (!option) return [];
  const t = option.target;
  return [t.space, t.toSpace].filter((s): s is string => typeof s === 'string' && s.length > 0);
}
