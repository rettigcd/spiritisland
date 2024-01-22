namespace SpiritIsland.Log;

public class LayoutChanged( string _text ) : ILogEntry {
	public LogLevel Level => LogLevel.Debug;

	public string Msg( LogLevel _ ) => _text;
}
