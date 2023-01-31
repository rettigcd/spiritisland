namespace SpiritIsland.Log;

public class Phase : ILogEntry {
	public SpiritIsland.Phase phase;
	public Phase( SpiritIsland.Phase phase ) {
		this.phase = phase;
		Level = LogLevel.Info;
	}
	public LogLevel Level { get; }
	public string Msg( LogLevel _ ) => $"-- {phase} --";
}
