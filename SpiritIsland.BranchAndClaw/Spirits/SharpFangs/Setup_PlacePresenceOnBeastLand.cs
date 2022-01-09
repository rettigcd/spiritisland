using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	class Setup_PlacePresenceOnBeastLand : GrowthActionFactory {

		public override async Task ActivateAsync( SelfCtx ctx ) {
			var gameState = ctx.GameState;
			var options = gameState.Island.AllSpaces.Where( space=>gameState.Tokens[space].Beasts.Any );
			var space = await ctx.Decision(new Select.Space("Add presence to",options, Present.Always));
			ctx.Presence.PlaceOn(space);
		}

		public override bool CouldActivateDuring( Phase speed, Spirit _ ) => speed == Phase.Init;

	}

}
