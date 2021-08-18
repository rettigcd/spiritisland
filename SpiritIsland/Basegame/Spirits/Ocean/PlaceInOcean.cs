using System.Linq;
using System.Threading.Tasks;
using SpiritIsland;

namespace SpiritIsland.Basegame {

	public class PlaceInOcean : GrowthActionFactory {

		public override Task Activate( Spirit spirit, GameState gameState ) {
			var oceanSpaces = gameState.Island.Boards
				.Select( b=>b.Spaces.Single(s=>s.IsOcean) )
				.ToArray();
			return spirit.MakeDecisionsFor(gameState).PlacePresence( oceanSpaces );
		}

	}

}
