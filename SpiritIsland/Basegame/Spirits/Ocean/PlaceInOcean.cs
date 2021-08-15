using System.Linq;
using System.Threading.Tasks;
using SpiritIsland;

namespace SpiritIsland.Basegame {

	public class PlaceInOcean : GrowthActionFactory {

		public override Task Activate( ActionEngine engine ) {
			var oceanSpaces = engine.GameState.Island.Boards
				.Select( b=>b.Spaces.Single(s=>s.IsOcean) )
				.ToArray();
			return engine.PlacePresence( oceanSpaces );
		}

	}

}
