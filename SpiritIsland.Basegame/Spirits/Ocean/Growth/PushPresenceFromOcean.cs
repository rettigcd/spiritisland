namespace SpiritIsland.Basegame;

public class PushPresenceFromOcean : SpiritAction {

	public PushPresenceFromOcean():base( "Push Presence from Ocean" ) { }

	public override async Task ActAsync( SelfCtx ctx ) {

		var pushSpaces = ctx.Self.Presence.Spaces
			.Where( p => p.IsOcean )
			.ToArray();

		foreach(var space in pushSpaces )
			await PushPresence( ctx, space );
	}

	static async Task PushPresence( SelfCtx ctx, Space from ) {
		var srcTokens = from.Tokens;
		var presenceTokens = ctx.Self.Presence.TokensDeployedOn( srcTokens ).Select( x => new SpaceToken( from, x ) ).ToArray();
		var token = await ctx.Decision( new Select.ASpaceToken( "Select presence to push", presenceTokens, Present.AutoSelectSingle ) );

		// #pushpresence
		Space destination = await ctx.Decision( Select.ASpace.PushPresence( from, srcTokens.Adjacent, Present.Always, token.Token ) );

		// apply...
		await token.MoveTo( destination );
	}

}