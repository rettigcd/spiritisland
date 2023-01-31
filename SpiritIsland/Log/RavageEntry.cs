namespace SpiritIsland.Log;

public class RavageEntry : ILogEntry {
	public InvadersRavaged Ravaged { get; }
	public RavageEntry( InvadersRavaged ravaged ) {
		Ravaged = ravaged;
		Level = LogLevel.Info;
	}
	public LogLevel Level { get; }
	public string Msg( LogLevel _ ) => Ravaged.ToString();
}
