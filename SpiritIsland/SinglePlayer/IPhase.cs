using System;
using SpiritIsland;

namespace SpiritIsland.SinglePlayer {

	interface IPhase : IDecisionStream {
		void Initialize();
		event Action Complete;
	}

}
