using System.Threading.Tasks;

namespace SpiritIsland {

	public class PlayExtraCardThisTurn : GrowthActionFactory {

		public override Task ActivateAsync( SpiritGameStateCtx ctx ) {
			ctx.Self.tempCardPlayBoost++;
			return Task.CompletedTask;
		}

	}

}
