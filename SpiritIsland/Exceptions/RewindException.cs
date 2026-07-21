using SpiritIsland.Log;

namespace SpiritIsland;

/// <summary>
/// Triggers Game-Engine task to throw an exception that the Game Engine loop catches, undoing the
/// action currently in progress and retrying it from the state immediately before it started.
/// </summary>
public class RewindException() : Exception, ILogEntry {

	LogLevel ILogEntry.Level => LogLevel.Info;

	string ILogEntry.Msg(LogLevel _) => "Rewind - undoing last action";
}
