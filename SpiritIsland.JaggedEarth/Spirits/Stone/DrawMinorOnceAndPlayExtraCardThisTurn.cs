using System.Threading.Tasks;

namespace SpiritIsland.JaggedEarth {

	public class DrawMinorOnceAndPlayExtraCardThisTurn : GrowthActionFactory {

		bool drewMinor = false;

		public override async Task ActivateAsync( SpiritGameStateCtx ctx ) {

			if(!drewMinor)
				await ctx.DrawMinor();
			drewMinor = true;

			ctx.Self.tempCardPlayBoost++;
		}

	}

}
