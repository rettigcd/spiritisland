namespace SpiritIsland.FeatherAndFlame;

public class EnergyForFire : GrowthActionFactory {

	public override Task ActivateAsync( SelfCtx ctx ) {
		ctx.Self.Energy += ctx.Self.Presence.TrackElements[Element.Fire];
		return Task.CompletedTask;
	}

}