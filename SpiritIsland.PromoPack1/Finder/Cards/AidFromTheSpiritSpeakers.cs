namespace SpiritIsland.PromoPack1;

public class AidFromTheSpiritSpeakers {

	[SpiritCard( "Aid From The Spirit-Speakers", 2, Element.Sun, Element.Fire, Element.Air )]
	[Fast, FromPresence( 1 )]
	static public async Task ActAsync( TargetSpaceCtx ctx ) {

		// For each dahan, you may move 1 Invader / dahan / presence / beast to a land within range - 2 that has dahan.
		await ctx.MoveTokensOut( max: ctx.Dahan.CountAll
			, ctx.TerrainMapper.Specify( 2, Target.Dahan )
			, Invader.Any.Plus(TokenType.Dahan,TokenType.Beast)
				.Union( ctx.AllPresenceTokens )
				.ToArray()
			);

	}

}

