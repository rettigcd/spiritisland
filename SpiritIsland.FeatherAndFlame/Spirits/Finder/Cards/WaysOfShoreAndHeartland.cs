namespace SpiritIsland.FeatherAndFlame;

public class WaysOfShoreAndHeartland {

	[SpiritCard( "Ways of Shore and Heartland", 1, Element.Air, Element.Water, Element.Earth ),Slow,FromPresence( 2 )]
	[Instructions( "Push up to 2 Invaders / Dahan / Presence / Beasts to a land that is also Coastal / Inland (whichever the target land is.)" ), Artist( Artists.MoroRogers )]
	static public async Task ActAsync( TargetSpaceCtx ctx ) {

		var tm = ActionScope.Current.TerrainMapper;

		// Push up to 2 Invaders / Dahan / Presence / Beast
		await ctx.Pusher
			.AddGroup(2, Human.Invader.Plus( Human.Dahan, Token.Beast ).Concat(ctx.AllPresenceTokens).ToArray() )
			// to a land that is also Coastal / Inland( whichever the target land is)
			.FilterDestinations( ctx.IsCoastal ? tm.IsCoastal : tm.IsInland )
			.MoveUpToN();
	}

}

