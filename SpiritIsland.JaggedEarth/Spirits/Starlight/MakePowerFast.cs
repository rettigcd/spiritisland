namespace SpiritIsland.JaggedEarth;

/// <summary>
/// Adds a ResolveSlowAsFast action.
/// </summary>
class MakePowerFast : SpiritAction {

	public MakePowerFast():base( "Make Power Fast" ) { }

	public override Task ActAsync( Spirit self ) {
		self.AddActionFactory( new ResolveSlowDuringFast() );
		return Task.CompletedTask;
	}

}
