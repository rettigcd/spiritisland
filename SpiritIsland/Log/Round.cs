namespace SpiritIsland.Log;

public class Round : ILogEntry {
	public int round;
	public Round( int round ) {
		this.round = round;
		Level = LogLevel.Info;
	}
	public LogLevel Level { get; }
	public string Msg( LogLevel _ ) => $"=== Round {round} ===";
}
