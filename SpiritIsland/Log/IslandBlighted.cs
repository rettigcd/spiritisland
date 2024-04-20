namespace SpiritIsland.Log;

public class IslandBlighted( BlightCard _card ) : ILogEntry { // event
	public LogLevel Level { get; } = LogLevel.Info;
	public BlightCard Card { get; } = _card;
	public string Msg( LogLevel _ ) => $"Blighted Island => {Card.Name} => {Card.Immediately.Description}\r\n  ^^^^^^^^ ^^^^^^\r\n";
}
