namespace SpiritIsland.JaggedEarth;

/// <summary>
/// Adds a ResolveSlowAsFast action.
/// </summary>
class MakePowerFast : GrowthActionFactory {

	public override Task ActivateAsync( SelfCtx ctx ) {
		ctx.Self.AddActionFactory( new ResolveSlowDuringFast() );
		return Task.CompletedTask;
	}

}
