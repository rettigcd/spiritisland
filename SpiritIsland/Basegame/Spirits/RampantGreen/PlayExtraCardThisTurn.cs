
using SpiritIsland.Core;

namespace SpiritIsland.BranchAndClaw {
	/// <summary>
	/// One of Rampant Green's special growth options
	/// </summary>
	class PlayExtraCardThisTurn : GrowthActionFactory {

		public override void Activate( ActionEngine engine ) {
			(engine.Self as RampantGreen).tempCardBoost++;
		}

	}

}
