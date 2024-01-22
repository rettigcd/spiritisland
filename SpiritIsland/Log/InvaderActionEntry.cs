namespace SpiritIsland.Log;

public class InvaderActionEntry( string _msg, LogLevel _level = LogLevel.Info ) : ILogEntry {
	public LogLevel Level { get; } = _level;
	public string Msg( LogLevel _ ) => _msg;
}
