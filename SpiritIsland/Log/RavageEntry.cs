namespace SpiritIsland.Log;

public class RavageEntry( params RavageExchange[] _exchange ) : ILogEntry {
	public RavageExchange[] Exchange { get; } = _exchange;
	public LogLevel Level { get; } = LogLevel.Info;
	public string Msg( LogLevel _ ) {
		string s = Exchange.Length == 0 ? "no-ravage"
			: Exchange[0].Space+": " + Exchange.Select(x=>x.ToString()).Join(";");
		return s;
	}
}
