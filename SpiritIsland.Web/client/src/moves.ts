import type { OptionTarget, OptionView } from './api/model';

/**
 * A move is one option that says both where it starts and where it ends ("E@1 on A1 => A2"), so once the player has picked
 * where a move starts, every place it could go is already known. These helpers group the current decision's moves by their
 * start, which is what lets the board offer moving as two steps (pick a token, then a land) or as a drag. What is sent to the
 * server is still the one option that matches both.
 */

/** Where a move starts: a token on the island, a slot of a presence track, or a spirit's pile of destroyed presence. */
export type SourceKey = string;

export const tokenSource = (space: string, token: string): SourceKey => `token:${space}|${token}`;
export const slotSource = (spirit: string, track: string, slot: number): SourceKey => `slot:${spirit}|${track}|${slot}`;
export const destroyedSource = (spirit: string): SourceKey => `destroyed:${spirit}`;

/** Where the move an option stands for starts; null if the option is not a move. */
export function sourceOf(target: OptionTarget): SourceKey | null {
  if (target.kind !== 'move') return null;
  if (target.space) return tokenSource(target.space, target.token ?? '');
  if (target.spirit && target.track && target.slot != null) return slotSource(target.spirit, target.track, target.slot);
  if (target.spirit) return destroyedSource(target.spirit);
  return null;
}

/** The moves of a decision, grouped by where they start (in the order they were offered). */
export function movesBySource(options: readonly OptionView[]): Map<SourceKey, OptionView[]> {
  const bySource = new Map<SourceKey, OptionView[]>();
  for (const option of options) {
    const key = sourceOf(option.target);
    if (key === null) continue;
    const moves = bySource.get(key);
    if (moves) moves.push(option);
    else bySource.set(key, [option]);
  }
  return bySource;
}

/**
 * Where moves that start in the same place can go on the island: each land, with the move that goes there. (If two moves go
 * to the same land, say two identical explorers, either will do.) A move that does not end on a land has no entry.
 */
export function destinationsOf(moves: readonly OptionView[]): Map<string, OptionView> {
  const byLand = new Map<string, OptionView>();
  for (const move of moves)
    if (move.target.toSpace && !byLand.has(move.target.toSpace)) byLand.set(move.target.toSpace, move);
  return byLand;
}

/** Where a move ends, for a list of destinations: the end of a move's label ("E@1 on A1 => A2" is "A2"). */
export function describeDestination(move: OptionView): string {
  const arrow = move.label.indexOf(' => ');
  return arrow < 0 ? move.label : move.label.slice(arrow + 4);
}

/** What is being moved, for a message: the start of a move's label ("E@1 on A1 => A2" is "E@1 on A1"). */
export function describeSource(moves: readonly OptionView[]): string {
  const label = moves[0]?.label ?? 'something';
  const arrow = label.indexOf(' => ');
  return arrow < 0 ? label : label.slice(0, arrow);
}
