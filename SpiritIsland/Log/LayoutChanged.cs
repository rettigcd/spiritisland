namespace SpiritIsland.Log;

/// <summary>
/// occurs when: (1) Island layout changes, (2) Spirits Growth changes, (3) Innate changes.
/// </summary>
/// <param name="_text"></param>
public class LayoutChanged( string _text ) : ILogEntry {
	public LogLevel Level => LogLevel.Debug;
	public string Msg( LogLevel _ ) => _text;
}
