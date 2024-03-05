namespace SpiritIsland.Log;

public class FearGenerated( int _number ) : ILogEntry {
	public LogLevel Level => LogLevel.Debug;
	public string Msg( LogLevel _ ) => $"{_number} Fear generated";
}

