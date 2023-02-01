namespace SpiritIsland.JaggedEarth;

public class LikeCallsToLike{ 

	[MinorCard("Like Calls to Like",1,Element.Sun,Element.Water,Element.Plant),Slow,FromPresence(1)]
	static public async Task ActAsync( TargetSpaceCtx ctx ) {
		// If target land has explorer, Gather up to 1 explorer.
		await GatherLike( ctx, Human.Explorer );
		// Do likewise for town, dahan, blight, and beast.
		await GatherLike( ctx, Human.Town );
		await GatherLike( ctx, Human.Dahan );
		await GatherLike( ctx, Token.Blight );
		await GatherLike( ctx, Token.Beast );
	}

	static async Task GatherLike( TargetSpaceCtx ctx, TokenClass tokenTypeOfInterest ) {
		if(ctx.Tokens.HasAny( tokenTypeOfInterest ))
			await ctx.GatherUpTo( 1, tokenTypeOfInterest );
	}

}