namespace SpiritIsland.PromoPack1;

public class PathsTiedByNature {

	[SpiritCard( "Paths Tied By Nature", 0, Element.Sun, Element.Air, Element.Earth, Element.Plant )]
	[Slow, FromPresence( 1 )]
	static public async Task ActAsync( TargetSpaceCtx ctx ) {

		// Move up to 2 Invaders / dahan / presence / beast to a land within range - 2 that has the same terrain.
		var currentTerrain = new[] { Target.Jungle, Target.Mountain, Target.Sand, Target.Wetland }
			.Where( t => SpaceFilterMap.MatchAny( ctx.Self, ctx.TerrainMapper, t )(ctx.Tokens) )
			.ToArray();

		await ctx.MoveTokensOut(2, ctx.TerrainMapper.Specify(2,currentTerrain)
			, new TokenClass[]{ TokenType.Dahan, TokenType.Beast, Invader.Explorer, Invader.Town, Invader.City }
				.Union( ctx.AllPresenceTokens ).ToArray()
		);
	}

}
