using System;

namespace SpiritIsland.FeatherAndFlame;

public class PathsTiedByNature {

	[SpiritCard( "Paths Tied By Nature", 0, Element.Sun, Element.Air, Element.Earth, Element.Plant )]
	[Slow, FromPresence( 1 )]
	static public async Task ActAsync( TargetSpaceCtx ctx ) {

		// Move up to 2 Invaders / dahan / presence / beast to a land within range - 2 that has the same terrain.
		var currentTerrain = new[] { Target.Jungle, Target.Mountain, Target.Sand, Target.Wetland }
			.Where( t => new TargetCriteria( 0, ctx.Self, t ).Matches(ctx.Tokens) )
		.ToArray();

		await ctx.MoveTokensOut(2, new TargetCriteria( 2, ctx.Self, currentTerrain )
			, Human.Invader.Plus( Human.Dahan, Token.Beast )
				.Union( ctx.AllPresenceTokens ).ToArray()
		);
	}

}
