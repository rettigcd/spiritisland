
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {
	class Setup_PlacePresenceInCostal : GrowthActionFactory {
		// ! Can't used normal PlacePresence, because it must be range-1, range 0 not allowed.
		public override async Task ActivateAsync( Spirit spirit, GameState gameState ) {
			var options = spirit.Presence.Spaces.First().Adjacent;
			var space = await spirit.Action.Decision( new Decision.TargetSpace( "Add presence to", options ) );
			spirit.Presence.PlaceOn( space );
		}
	}

}
