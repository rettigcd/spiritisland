namespace SpiritIsland.JaggedEarth;

/// <summary>
/// Adds a ResolveSlowAsFast action.
/// </summary>
class MakePowerFast : SpiritAction {

	public MakePowerFast():base( "MakePowerFast" ) { }

	public override Task ActAsync( SelfCtx ctx ) {
		ctx.Self.AddActionFactory( new ResolveSlowDuringFast() );
		return Task.CompletedTask;
	}

}
