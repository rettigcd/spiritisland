namespace SpiritIsland.Log;

public class BlightOnCardChanged : ILogEntry {
	public LogLevel Level => LogLevel.Debug;
	public string Msg(LogLevel _) => $"Blight on card changed.";
}
