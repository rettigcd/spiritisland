namespace SpiritIsland.Log;

public class LayoutChanged : ILogEntry {
	public LayoutChanged( string text ) { this.text = text; }
	readonly string text;
	public LogLevel Level => LogLevel.Debug;

	public string Msg( LogLevel _ ) => text;
}
