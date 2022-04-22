namespace SpiritIsland;

public class ReclaimAll : GrowthActionFactory {

	public override Task ActivateAsync( SelfCtx ctx ) {
		var spirit = ctx.Self;
		spirit.Hand.AddRange( spirit.DiscardPile );
		spirit.DiscardPile.Clear();
		return Task.CompletedTask;
	}

	public override bool AutoRun => true;

}