namespace SpiritIsland.PromoPack1;

public class EnergyForFire : GrowthActionFactory {

	public override Task ActivateAsync( SelfCtx ctx ) {
		ctx.Self.Energy += ctx.Self.Presence.AddElements()[Element.Fire];
		return Task.CompletedTask;
	}

}