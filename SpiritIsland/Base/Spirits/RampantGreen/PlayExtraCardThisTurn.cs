
using SpiritIsland.Core;

namespace SpiritIsland.BranchAndClaw {
	/// <summary>
	/// One of Rampant Green's special growth options
	/// </summary>
	class PlayExtraCardThisTurn : GrowthActionFactory {

		public override IAction Bind( Spirit spirit, GameState gameState ) {
			(spirit as RampantGreen).tempCardBoost++;
			return new ResolvedAction();
		}

	}

}
