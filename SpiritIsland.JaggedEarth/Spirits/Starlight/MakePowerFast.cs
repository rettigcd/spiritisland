namespace SpiritIsland.JaggedEarth;

/// <summary>
/// Adds a ResolveSlowAsFast action.
/// </summary>
class MakePowerFast : SpiritAction {

	public MakePowerFast():base( "MakePowerFast" ) { }

	public override Task ActAsync( Spirit self ) {
		self.AddActionFactory( new ResolveSlowDuringFast() );
		return Task.CompletedTask;
	}

}
