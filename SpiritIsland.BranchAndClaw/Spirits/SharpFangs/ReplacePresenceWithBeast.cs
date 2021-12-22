using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	class ReplacePresenceWithBeast : GrowthActionFactory {

		public override async Task ActivateAsync( SelfCtx ctx ) {
			if(ctx.Self.Presence.Placed.Count==1) return; // don't let them switch their last presence to a beast
			var options = ctx.Self.Presence.Spaces;
			var space = await ctx.Decision(new Select.DeployedPresence("Select presence to replace with beast",options,Present.Done)); // let them change their minds
			if(space == null) return;

			await ctx.Presence.RemoveFrom(space);
			await ctx.GameState.Tokens[space].Beasts.Add(1);

		}

	}

}
