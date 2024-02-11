using SpiritIsland.Log;

namespace SpiritIsland;

/// <summary>
/// Contains result (win/loss) and cause (string)
/// </summary>
public class GameOverLogEntry( GameOverResult result, string cause ) : ILogEntry {
	public GameOverResult Result { get; } = result;

	public string Cause { get; } = cause;

	#region ILogEnergy imp

	string ILogEntry.Msg( LogLevel _ ) => ToString();

	LogLevel ILogEntry.Level => LogLevel.Info;

	#endregion ILogEnergy imp

	public override string ToString() => $"{Result}!: {Cause}";
}