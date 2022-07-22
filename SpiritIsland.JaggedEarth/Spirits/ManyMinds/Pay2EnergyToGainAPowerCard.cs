namespace SpiritIsland.JaggedEarth;

class Pay2EnergyToGainAPowerCard : GrowthActionFactory, IActionFactory {

	public override async Task ActivateAsync( SelfCtx ctx ) {
		if( 2<=ctx.Self.Energy && await ctx.Self.UserSelectsFirstText("Draw Power Card?", "Yes, pay 2 energy", "No, thank you.")) {
			ctx.Self.Energy -= 2;
			await ctx.Draw();
		}
	}

}