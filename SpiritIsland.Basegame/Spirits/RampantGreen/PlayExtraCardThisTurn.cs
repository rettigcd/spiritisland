using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	/// <summary>
	/// One of Rampant Green's special growth options
	/// </summary>
	class PlayExtraCardThisTurn : GrowthActionFactory {

		public override Task ActivateAsync( SpiritGameStateCtx ctx ) {
			(ctx.Self as ASpreadOfRampantGreen).tempCardPlayBoost++; // !!! move this into Spirit so other spirits can do it
			return Task.CompletedTask;
		}

	}

}
