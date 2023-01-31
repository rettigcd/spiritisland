namespace SpiritIsland.Log;

public class Debug : ILogEntry {
	public Debug( string text ) { this.text = text; }
	readonly string text;
	public LogLevel Level => LogLevel.Debug;

	public string Msg( LogLevel _ ) => text;
}
