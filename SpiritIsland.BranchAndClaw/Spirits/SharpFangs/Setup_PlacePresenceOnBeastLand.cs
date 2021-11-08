using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	class Setup_PlacePresenceOnBeastLand : GrowthActionFactory {

		public override async Task ActivateAsync( SpiritGameStateCtx ctx ) {
			var gameState = ctx.GameState;
			var options = gameState.Island.AllSpaces.Where( space=>gameState.Tokens[space].Beasts.Any );
			var space = await ctx.Self.Action.Decision(new Decision.TargetSpace("Add presence to",options, Present.Always));
			ctx.Presence.PlaceOn(space);
		}

	}

}
