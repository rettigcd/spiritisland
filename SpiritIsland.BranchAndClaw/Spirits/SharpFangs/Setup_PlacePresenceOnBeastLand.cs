using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	class Setup_PlacePresenceOnBeastLand : GrowthActionFactory {

		public override async Task ActivateAsync( Spirit spirit, GameState gameState ) {
			var options = gameState.Island.AllSpaces.Where( space=>gameState.Tokens[space].Beasts().Any );
			var space = await spirit.Action.Decision(new Decision.TargetSpace("Add presence to",options));
			spirit.Presence.PlaceOn(space);
		}

	}

}
