using SpiritIsland.Basegame;
using SpiritIsland.SinglePlayer;

namespace SpiritIsland.Tests.Basegame.Spirits.River {

	public class RiverGame : DecisionTests {

		public RiverGame():base(new RiverSurges() ) { }

		protected SinglePlayerGame game;
	}

}
