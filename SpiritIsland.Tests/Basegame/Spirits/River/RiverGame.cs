using SpiritIsland.Basegame;
using SpiritIsland.SinglePlayer;

namespace SpiritIsland.Tests.Basegame.Spirits.River {

	public class RiverGame {

		public RiverGame() {
			spirit = new RiverSurges();
			User = new VirtualRiverUser( spirit );
		}

		protected SinglePlayerGame game;
		protected readonly Spirit spirit;
		protected readonly VirtualRiverUser User;
	}

}
