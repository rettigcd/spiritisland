using System.Threading.Tasks;

namespace SpiritIsland.JaggedEarth {
	class Pay2EnergyToGainAPowerCard : GrowthActionFactory {
		public override async Task ActivateAsync( SpiritGameStateCtx ctx ) {
			if( 2<=ctx.Self.Energy && await ctx.Self.UserSelectsFirstText("Pay 2 energy for +1 card play?", "Yes", "No, thank you.")) {
				ctx.Self.Energy -= 2;
				ctx.Self.tempCardPlayBoost++;
			}
		}
	}

}
