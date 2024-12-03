namespace SpiritIsland.Log;

public class BlightOnCardChanged(int newCount) : ILogEntry {
	public int NewCount => newCount;
	public LogLevel Level => LogLevel.Debug;
	public string Msg(LogLevel _) => $"Blight on card changed to {newCount}.";
}
