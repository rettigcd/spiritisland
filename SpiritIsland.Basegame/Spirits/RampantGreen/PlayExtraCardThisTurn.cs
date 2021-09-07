using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	/// <summary>
	/// One of Rampant Green's special growth options
	/// </summary>
	class PlayExtraCardThisTurn : GrowthActionFactory {

		public override Task ActivateAsync( Spirit self, GameState _ ) {
			(self as ASpreadOfRampantGreen).tempCardBoost++;
			return Task.CompletedTask;
		}

	}

}
