using System;
using SpiritIsland.Core;

namespace SpiritIslandCmd {

	interface IPhase : IDecision {
		void Initialize();
		event Action Complete;
	}

}
