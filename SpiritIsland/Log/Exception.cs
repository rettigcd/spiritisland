namespace SpiritIsland.Log;

public class Exception : ILogEntry {
	public System.Exception ex;
	public Exception( System.Exception ex ) {
		this.ex = ex;
		Level = LogLevel.Fatal;
	}
	public LogLevel Level { get; }
	public string Msg( LogLevel _ ) => ex.ToString();
}
