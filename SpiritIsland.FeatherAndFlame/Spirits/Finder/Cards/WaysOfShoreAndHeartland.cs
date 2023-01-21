namespace SpiritIsland.FeatherAndFlame;

public class WaysOfShoreAndHeartland {

	[SpiritCard( "Ways Of Shore And Heartland", 1, Element.Air, Element.Water, Element.Earth )]
	[Slow,FromPresence( 2 )]
	static public async Task ActAsync( TargetSpaceCtx ctx ) {

		// Push up to 2 Invaders / Dahan / Presence / Beast
		await ctx.Pusher
			.AddGroup(2, Invader.Any.Plus( TokenType.Dahan, TokenType.Beast ).Concat(ctx.AllPresenceTokens).ToArray() )
			// to a land that is also Coastal / Inland( whichever the target land is)
			.FilterDestinations( ctx.IsCoastal ? ctx.TerrainMapper.IsCoastal : ctx.TerrainMapper.IsInland )
			.MoveUpToN();
	}

}

