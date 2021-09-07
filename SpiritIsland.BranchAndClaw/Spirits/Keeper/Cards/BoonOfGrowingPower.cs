using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	public class BoonOfGrowingPower {

		// target spirit
		[SpiritCard("Boon of Growing Power",1,Speed.Slow,Element.Sun,Element.Moon,Element.Plant)]
		[TargetSpirit]
		static public async Task ActAsync( TargetSpiritCtx ctx ) {

			// target spirit gains a power card
			await ctx.Target.Draw(ctx.GameState, null);

			// if you target another spirit, they also gain 1 energy
			if(ctx.Target != ctx.Self)
				++ctx.Target.Energy;

		}

	}

}
