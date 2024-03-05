namespace SpiritIsland;

public class PlayExtraCardThisTurn( int count ) : SpiritAction(), ICanAutoRun {
	public override string Description => $"Play {Count} extra Card{Plural(count)} this turn";
	static string Plural( int count ) => count == 1 ? "" : "s";
	public override Task ActAsync( Spirit self ) {
		self.TempCardPlayBoost += Count;
		return Task.CompletedTask;
	}
	public int Count => count;
}
