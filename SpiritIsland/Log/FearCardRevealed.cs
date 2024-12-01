namespace SpiritIsland.Log;

public class FearCardRevealed(IFearCard card) : ILogEntry {

	public IFearCard Card { get; } = card; public LogLevel Level => LogLevel.Info;
	public string Msg(LogLevel _) => $"{Card.Text} : {GetInstructions()}";

	public string GetInstructions() {
		if( Card.ActivatedTerrorLevel.HasValue )
			return $"{Card.ActivatedTerrorLevel.Value} : {Card.GetDescription()}";

		// Show all Levels
		List<string> parts = [];
		for (int i = 1; i <= 3; ++i)
			parts.Add($"{i} : {Card.GetDescription(i)}");
		return parts.Join("\r\n");
		
	}

}
