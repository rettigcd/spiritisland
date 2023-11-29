namespace SpiritIsland.NatureIncarnate;

public class AddVitalityToIncarna : SpiritAction {

	public AddVitalityToIncarna():base( "AddVitalityToIncarna" ) { }
	public override async Task ActAsync( SelfCtx ctx ) {
		if(ctx.Self is ToweringRootsOfTheJungle roots && roots.Incarna.Space is not null)
			await roots.Incarna.Space.AddAsync(Token.Vitality,1);
	}
	// public override bool AutoRun => true; don't override in case we want to move incarna first, then add vitality
}
