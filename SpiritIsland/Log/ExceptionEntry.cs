namespace SpiritIsland.Log;

public class ExceptionEntry : ILogEntry {
	public System.Exception ex;
	public ExceptionEntry( System.Exception ex ) {
		this.ex = ex;
		Level = LogLevel.Fatal;
	}
	public LogLevel Level { get; }
	public string Msg( LogLevel _ ) => ex.ToString();
}
