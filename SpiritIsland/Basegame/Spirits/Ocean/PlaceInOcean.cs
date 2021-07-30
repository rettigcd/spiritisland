using System.Linq;
using SpiritIsland.Core;

namespace SpiritIsland.Basegame {

	public class PlaceInOcean : GrowthActionFactory {

		public override IAction Bind( Spirit spirit, GameState gameState ) {
			var targetOptions = gameState.Island.Boards
			.Select( b=>b.Spaces.Single(s=>s.IsOcean) )
			.ToArray();
			return new PlacePresenceBaseAction(spirit,gameState,targetOptions);
		}

	}

}
