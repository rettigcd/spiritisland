using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	class ReplacePresenceWithBeast : GrowthActionFactory {

		public override async Task ActivateAsync( SpiritGameStateCtx ctx ) {
			if(ctx.Self.Presence.Placed.Count==1) return; // don't let them switch their last presence to a beast
			var options = ctx.Self.Presence.Spaces;
			var space = await ctx.Self.Action.Decision(new Decision.Presence.Deployed("Select presence to replace with beast",options,Present.Done)); // let them change their minds
			if(space == null) return;

			ctx.Presence.RemoveFrom(space);
			ctx.GameState.Tokens[space].Beasts.Count++;

		}

	}

}
