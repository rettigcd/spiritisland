namespace SpiritIsland.BranchAndClaw;

public class ReplacePresenceWithBeast : GrowthActionFactory {

	public override async Task ActivateAsync( SelfCtx ctx ) {
		if(ctx.Self.Presence.TotalOnIsland()==1) return; // don't let them switch their last presence to a beast

		var spaceToken = await ctx.Decision( new Select.ASpaceToken( "Select presence to replace with beast", ctx.Self.Presence.Deployed, Present.Done ) ); // let them change their minds
		if(spaceToken == null) return;

		await spaceToken.Remove();
		await spaceToken.Space.Tokens.Beasts.AddAsync(1);

	}

}