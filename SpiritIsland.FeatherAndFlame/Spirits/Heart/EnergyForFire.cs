namespace SpiritIsland.FeatherAndFlame;

public class EnergyForFire : SpiritAction {

	public EnergyForFire() : base( "EnergyForFire" ) { }

	public override Task ActAsync( SelfCtx ctx ) {
		ctx.Self.Energy += ctx.Self.Presence.TrackElements[Element.Fire];
		return Task.CompletedTask;
	}

}