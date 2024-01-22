namespace SpiritIsland.Log;

public class Phase( SpiritIsland.Phase phase ) : ILogEntry {
	public SpiritIsland.Phase phase = phase;

	public LogLevel Level { get; } = LogLevel.Info;
	public string Msg( LogLevel _ ) => $"-- {phase} --";
}
