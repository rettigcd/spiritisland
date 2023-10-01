namespace SpiritIsland.Basegame;

public class PushPresenceFromOcean : GrowthActionFactory {

	public override async Task ActivateAsync( SelfCtx ctx ) {

		var pushSpaces = ctx.Self.Presence.Spaces
			.Where( p => p.IsOcean )
			.Distinct()
			.ToList();

		while(0 < pushSpaces.Count){
			var currentSource = pushSpaces[0];
			var srcTokens = currentSource.Tokens;
			var token = ctx.Self.Presence.TokensDeployedOn(srcTokens).First();

			// #pushpresence
			Space destination = await ctx.Decision( Select.ASpace.PushPresence( currentSource, srcTokens.Adjacent, Present.Always, token ) );

			// apply...
			await token.Move( srcTokens, destination );

			// next
			pushSpaces.RemoveAt( 0 );
		}

	}

}