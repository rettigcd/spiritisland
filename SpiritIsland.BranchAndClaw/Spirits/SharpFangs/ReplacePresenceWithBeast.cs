using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {
	class ReplacePresenceWithBeast : GrowthActionFactory {

		public override async Task ActivateAsync( Spirit spirit, GameState gameState ) {
			if(spirit.Presence.Placed.Count==1) return; // don't let them switch their last presence to a beast
			var options = spirit.Presence.Spaces;
			var space = await spirit.Action.Decide(new TargetSpaceDecision("Select presence to replace with beast",options,Present.Done)); // let them change their minds
			if(space == null) return;

			spirit.Presence.RemoveFrom(space);
			gameState.Tokens[space].Beasts().Count++;

		}

	}

}
