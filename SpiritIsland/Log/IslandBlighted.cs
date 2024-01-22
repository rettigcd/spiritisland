namespace SpiritIsland.Log;

public class IslandBlighted( IBlightCard _card ) : ILogEntry { // event
	public LogLevel Level { get; } = LogLevel.Info;
	public IBlightCard Card { get; } = _card;
	public string Msg( LogLevel _ ) => $"Blighted Island => {Card.Name} => {Card.Immediately.Description}\r\n  ^^^^^^^^ ^^^^^^\r\n";
}
