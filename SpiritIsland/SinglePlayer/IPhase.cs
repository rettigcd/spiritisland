using System;
using SpiritIsland;

namespace SpiritIsland.SinglePlayer {

	interface IPhase {
		void Initialize();
		event Action Complete;
	}

}
