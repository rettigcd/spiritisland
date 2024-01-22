using SpiritIsland.Log;

namespace SpiritIsland;

/// <summary>
/// Logable Status
/// </summary>
public class GameOver( GameOverResult result, string cause ) : ILogEntry {
	public GameOverResult Result { get; } = result;

	public string Cause { get; } = cause;

	public string Msg( LogLevel _ ) => $"{Result}!: {Cause}";

	public LogLevel Level => LogLevel.Info;
}