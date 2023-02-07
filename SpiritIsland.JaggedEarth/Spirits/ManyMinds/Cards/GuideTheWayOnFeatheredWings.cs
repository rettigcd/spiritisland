namespace SpiritIsland.JaggedEarth;

public class GuideTheWayOnFeatheredWings {

	[SpiritCard("Guide the Way on Feathered Wings", 0, Element.Sun,Element.Air,Element.Animal), Fast, FromPresence(1)]
	static public Task ActAsync(TargetSpaceCtx ctx ) {
		// Move 1 beast  up to two lands.
		return MoveBeastAndFriends( ctx, 2 );

	}

	static public async Task MoveBeastAndFriends(TargetSpaceCtx ctx, int steps ) {
		if(steps <= 0 ) return;

		// move beast
		Space destination = await ctx.MoveTokensOut(1, ctx.TerrainMapper.Specify( 1 ), Token.Beast );
		if(destination == null) return;
			
		// As it moves, up to 2 dahan may move with it, for part or all of the way.
		// the beast / dahan may move to an adjacent land and then back.
		var destCtx = ctx.Target(destination);
		await new TokenCollectorFromSpecifiedSources(destCtx, ctx.Tokens)
			.AddGroup(2,Human.Dahan)
			.CollectUpToN();

		await MoveBeastAndFriends(destCtx,steps-1);
	}

}
