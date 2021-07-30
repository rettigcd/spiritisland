using SpiritIsland.Core;
using System.Linq;

namespace SpiritIsland.Basegame {

	public class OverseasTradeSeemSafer : IFearCard {

		// "Defend 3 in all Coastal lands.", 
		public void Level1( GameState gs ) {
			foreach(var space in gs.Island.AllSpaces.Where(s=>s.IsCostal))
				gs.Defend(space,3);
			// !!! this method has no unit tests
		}

		// "Defend 6 in all Coastal lands. Invaders do not Build City in Coastal lands this turn.", 
		public void Level2( GameState gs ) {
			throw new System.NotImplementedException();
		}

		// "Defend 9 in all Coastal lands. Invaders do not Build in Coastal lands this turn."),
		public void Level3( GameState gs ) {
			throw new System.NotImplementedException();
		}
	}
}
