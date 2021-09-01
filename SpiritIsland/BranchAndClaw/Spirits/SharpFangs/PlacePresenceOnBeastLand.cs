using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {
	class PlacePresenceOnBeastLand : GrowthActionFactory {
		public override async Task ActivateAsync( Spirit spirit, GameState gameState ) {
			var gsbac = (GameState_BranchAndClaw)gameState;
			var options = gameState.Island.AllSpaces.Where(gsbac.Beasts.AreOn);
			var space = await spirit.Action.Choose(new TargetSpaceDecision("Add presence to",options));
			spirit.Presence.PlaceOn(space);
		}
	}

}
