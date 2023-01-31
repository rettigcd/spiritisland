namespace SpiritIsland.Log;

public class InvaderActionEntry : ILogEntry {
	public string msg;
	public InvaderActionEntry( string msg, LogLevel level = LogLevel.Info ) {
		this.msg = msg;
		Level = level;
	}
	public LogLevel Level { get; }
	public string Msg( LogLevel _ ) => msg;
}
