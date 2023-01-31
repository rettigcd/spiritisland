namespace SpiritIsland.Log;

public class IslandBlighted : ILogEntry { // event
	public IslandBlighted( IBlightCard card ) {
		Level = LogLevel.Info;
		Card = card;
	}
	public LogLevel Level { get; }
	public IBlightCard Card { get; }
	public string Msg( LogLevel _ ) => $"Blighted Island => {Card.Name} => {Card.Immediately.Description}\r\n  ^^^^^^^^ ^^^^^^\r\n";
}
