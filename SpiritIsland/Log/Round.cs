namespace SpiritIsland.Log;

public class Round( int _number ) : ILogEntry {
	public int Number = _number;

	public LogLevel Level { get; } = LogLevel.Info;
	public string Msg( LogLevel _ ) => $"=== Round {Number} ===";
}
