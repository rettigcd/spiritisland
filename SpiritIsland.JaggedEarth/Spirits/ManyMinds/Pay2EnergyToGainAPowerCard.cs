namespace SpiritIsland.JaggedEarth;

class Pay2EnergyToGainAPowerCard : SpiritAction {

	public Pay2EnergyToGainAPowerCard():base( "Pay2EnergyToGainAPowerCard" ) { }

	public override async Task ActAsync( SelfCtx ctx ) {
		if( 2<=ctx.Self.Energy && await ctx.Self.UserSelectsFirstText("Draw Power Card?", "Yes, pay 2 energy", "No, thank you.")) {
			ctx.Self.Energy -= 2;
			await ctx.Draw();
		}
	}

}