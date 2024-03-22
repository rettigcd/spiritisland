using SpiritIsland.Log;

namespace SpiritIsland;

/// <summary>
/// Triggers Game-Engine task to throw an exception that the Game Engine loop catches and restarts at beginning of round.
/// </summary>
/// <param name="targetRound"></param>
public class RewindException( int targetRound ) : Exception, ILogEntry {
	public int TargetRound { get; } = targetRound;

	LogLevel ILogEntry.Level => LogLevel.Info;

	string ILogEntry.Msg(LogLevel _) => $"Rewind to start of round {TargetRound}";
}
