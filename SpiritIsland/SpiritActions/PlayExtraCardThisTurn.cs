namespace SpiritIsland;

public class PlayExtraCardThisTurn : SpiritAction, ICanAutoRun {

	public PlayExtraCardThisTurn( int count ):base()
	{
		Count = count;
	}
	public override string Description => $"Play {Count} extra Card(s) this turn.";
	public override Task ActAsync( Spirit self ) {
		self.TempCardPlayBoost += Count;
		return Task.CompletedTask;
	}
	public int Count { get; }
}
