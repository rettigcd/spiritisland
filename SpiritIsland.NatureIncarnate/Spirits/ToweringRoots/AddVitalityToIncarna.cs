namespace SpiritIsland.NatureIncarnate;

public class AddVitalityToIncarna : SpiritAction {

	public AddVitalityToIncarna():base( "AddVitalityToIncarna" ) { }
	public override async Task ActAsync( Spirit self ) {
		if(self is ToweringRootsOfTheJungle roots && roots.Incarna.IsPlaced)
			await roots.Incarna.Space.AddAsync(Token.Vitality,1);
	}
	// public override bool AutoRun => true; don't override in case we want to move incarna first, then add vitality
}
