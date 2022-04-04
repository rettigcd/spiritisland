namespace SpiritIsland;

public class GameOver : ILogEntry {

	public GameOver( GameOverResult result, string cause ) { 
		Result = result;
		Cause = cause;
	}

	public GameOverResult Result { get; }

	public string Cause { get; }

	public string Msg( LogLevel _ ) => $"{Result}!: {Cause}";

	public LogLevel Level => LogLevel.Info;
}