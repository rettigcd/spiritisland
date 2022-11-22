namespace SpiritIsland.BranchAndClaw;

public class ReplacePresenceWithBeast : GrowthActionFactory {

	public override async Task ActivateAsync( SelfCtx ctx ) {
		if(ctx.Self.Presence.Placed(ctx.GameState).Count==1) return; // don't let them switch their last presence to a beast
		var options = ctx.Presence.Spaces;
		var space = await ctx.Decision(new Select.DeployedPresence("Select presence to replace with beast",options,Present.Done)); // let them change their minds
		if(space == null) return;

		await ctx.Presence.RemoveFrom(space);
		await ctx.GameState.Tokens[space].Beasts.Bind(ctx.CurrentActionId).Add(1);

	}

}