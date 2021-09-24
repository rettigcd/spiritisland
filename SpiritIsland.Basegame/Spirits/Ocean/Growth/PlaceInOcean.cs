using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class PlaceInOcean : GrowthActionFactory {

		public override Task ActivateAsync( Spirit spirit, GameState gameState ) {
			var oceanSpaces = gameState.Island.Boards
				.Select( b=>b.Spaces.Single(s=>s.Terrain == Terrain.Ocean ) )
				.ToArray();
			return spirit.MakeDecisionsFor(gameState).PlacePresence( oceanSpaces );
		}

	}

}
