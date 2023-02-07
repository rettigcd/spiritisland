namespace SpiritIsland.BranchAndClaw;

public class ReplacePresenceWithBeast : GrowthActionFactory {

	public override async Task ActivateAsync( SelfCtx ctx ) {
		if(ctx.Self.Presence.Total(ctx.GameState)==1) return; // don't let them switch their last presence to a beast
		var options = ctx.Presence.ActiveSpaceStates;
		var space = await ctx.Decision(new Select.Space( "Select presence to replace with beast",options,Present.Done, ctx.Self.Presence.Token)); // let them change their minds
		if(space == null) return;

		await ctx.Presence.RemoveFrom(space);
		await ctx.GameState.Tokens[space].Beasts.BindScope().Add(1);

	}

}