namespace SpiritIsland;

/// <summary>
/// Triggers Game-Engine task to throw an exception that the Game Engine loop catches and restarts at beginning of round.
/// </summary>
/// <param name="targetRound"></param>
public class RewindException( int targetRound ) : Exception() {
	public int TargetRound { get; } = targetRound;
}
