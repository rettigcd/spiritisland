namespace SpiritIsland;

public class PlayExtraCardThisTurn : SpiritAction, ICanAutoRun {

	public PlayExtraCardThisTurn( int count ):base()
	{
		Count = count;
	}
	public override string Description => $"Play {Count} extra Card(s) this turn.";
	public override Task ActAsync( SelfCtx ctx ) {
		ctx.Self.tempCardPlayBoost += Count;
		return Task.CompletedTask;
	}
	public int Count { get; }
}
