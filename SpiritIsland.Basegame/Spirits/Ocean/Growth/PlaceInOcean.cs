using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class PlaceInOcean : GrowthActionFactory {

		public override Task ActivateAsync( SpiritGameStateCtx ctx ) {
			var oceanSpaces = ctx.GameState.Island.Boards
				.Select( b=>b.Spaces.Single(s=>s.Terrain == Terrain.Ocean ) )
				.ToArray();
			return ctx.PlacePresence( oceanSpaces );
		}

	}

}
