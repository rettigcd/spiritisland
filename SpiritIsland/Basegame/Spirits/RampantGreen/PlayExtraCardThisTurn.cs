
using SpiritIsland.Core;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {
	/// <summary>
	/// One of Rampant Green's special growth options
	/// </summary>
	class PlayExtraCardThisTurn : GrowthActionFactory {

		public override Task Activate( ActionEngine engine ) {
			(engine.Self as ASpreadOfRampantGreen).tempCardBoost++;
			return Task.CompletedTask;
		}

	}

}
