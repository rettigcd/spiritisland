namespace SpiritIsland.NatureIncarnate;

public class AddVitalityToIncarna : GrowthActionFactory {
	public override async Task ActivateAsync( SelfCtx ctx ) {
		if(ctx.Self is ToweringRootsOfTheJungle roots && roots.Incarna.Space is not null)
			await roots.Incarna.Space.Add(Token.Vitality,1);
	}
	// public override bool AutoRun => true; don't override in case we want to move incarna first, then add vitality
}
