using System.Threading.Tasks;

namespace SpiritIsland.JaggedEarth {

	public class DrawMinorOnceAndPlayExtraCardThisTurn : GrowthActionFactory, ITrackActionFactory {

		bool drewMinor = false;

		public bool RunAfterGrowthResult => false; // no growth dependencies

		public override async Task ActivateAsync( SpiritGameStateCtx ctx ) {

			if(!drewMinor)
				await ctx.DrawMinor();
			drewMinor = true;

			ctx.Self.tempCardPlayBoost++;
		}

	}

}
