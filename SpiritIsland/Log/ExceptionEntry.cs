namespace SpiritIsland.Log;

public class ExceptionEntry( Exception _ex ) : ILogEntry {
	public System.Exception Ex = _ex;

	public LogLevel Level { get; } = LogLevel.Fatal;
	public string Msg( LogLevel _ ) => Ex.ToString();
}