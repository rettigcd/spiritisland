namespace SpiritIsland.Log;

public class RavageEntry : ILogEntry {
	public RavageExchange[] Exchange { get; }
	public RavageEntry( params RavageExchange[] exchange ) {
		Exchange = exchange;
		Level = LogLevel.Info;
	}
	public LogLevel Level { get; }
	public string Msg( LogLevel _ ) {
		string s = Exchange.Length == 0 ? "no-ravage"
			: Exchange[0].Space+": " + Exchange.Select(x=>x.ToString()).Join(";");
		return s;
	}
}
