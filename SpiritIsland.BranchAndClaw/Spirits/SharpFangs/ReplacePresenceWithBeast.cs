using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {
	class ReplacePresenceWithBeast : GrowthActionFactory {

		public override async Task ActivateAsync( SpiritGameStateCtx ctx ) {
			var spirit = ctx.Self;
			if(spirit.Presence.Placed.Count==1) return; // don't let them switch their last presence to a beast
			var options = spirit.Presence.Spaces;
			var space = await spirit.Action.Decision(new Decision.Presence.Deployed("Select presence to replace with beast",options,Present.Done)); // let them change their minds
			if(space == null) return;

			spirit.Presence.RemoveFrom(space);
			ctx.GameState.Tokens[space].Beasts.Count++;

		}

	}

}
