using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class PlaceInOcean : GrowthActionFactory {

		public override Task ActivateAsync( SelfCtx ctx ) {
			var oceanSpaces = ctx.GameState.Island.Boards
				.Select( b=>b.Spaces.Single(s=>s.IsOcean ) )
				.ToArray();
			return ctx.Presence.Place( oceanSpaces );
		}

	}

}
