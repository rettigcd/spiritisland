using System;
using System.Threading.Tasks;
using SpiritIsland;

namespace SpiritIsland.SinglePlayer {

	interface IPhase {
		void Initialize();
		event Action Complete;

		Task ActAsync();
	}

}
