namespace SpiritIsland.JaggedEarth;

class Gain1EnergyOnReveal : GrowthActionFactory, IActionFactory {
	bool ran;

	public override Task ActivateAsync( SelfCtx ctx ) {
		if(!ran) { 
			ctx.Self.Energy++;
			ran = true;
		}
		return Task.CompletedTask;
	}
}
