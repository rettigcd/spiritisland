namespace SpiritIsland.BranchAndClaw;

public class ReplacePresenceWithBeast : GrowthActionFactory {

	public override async Task ActivateAsync( SelfCtx ctx ) {
		if(ctx.Self.Presence.TotalOnIsland()==1) return; // don't let them switch their last presence to a beast
		var options = ctx.Self.Presence.Spaces;
		var space = await ctx.Decision(new Select.ASpace( "Select presence to replace with beast",options.Tokens(),Present.Done, ctx.Self.Token)); // let them change their minds
		if(space == null) return;

		await ctx.Self.Token.RemoveFrom(space);
		await space.Tokens.Beasts.Add(1);

	}

}