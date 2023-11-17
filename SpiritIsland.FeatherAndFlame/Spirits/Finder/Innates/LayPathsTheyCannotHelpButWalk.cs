namespace SpiritIsland.FeatherAndFlame;

// fast, range 0, ANY
// 2 moon, 2 air Push up to half (rounded down) of Invaders from target land.  Do likewise for Dahan, Presence, and Beast (each separately).
// 2 sun, 2 air Push up to 1 Invader/Dahan/Presence/Beast.
// 2 moon, 4 air, 3 water Repeat this Power


[InnatePower( "Lay Paths They Cannot Help But Walk" ), Fast]
[FromPresence( 0 )]
[RepeatIf("2 moon,4 air,3 water")]
public class LayPathsTheyCannotHelpButWalk {

	[InnateTier( "2 moon,2 air", "Push up to half (rounded down) of Invaders from target land. Do likewise for dahan, presence, and beast (each separately)." )]
	static async public Task Option1( TargetSpaceCtx ctx ) {

		var pusher = ctx.Pusher;

		// Push up to half( rounded down ) of Invaders from target land.
		AddHalf(pusher, ctx.Tokens, Human.Invader );
		// Do likewise for dahan
		AddHalf(pusher, ctx.Tokens, Human.Dahan);
		// Presence
		AddHalf( pusher, ctx.Tokens, ctx.AllPresenceTokens );
		// and beast( each separately ).
		AddHalf( pusher, ctx.Tokens, Token.Beast );

		await pusher.DoUpToN();
	}

	static void AddHalf( TokenMover pusher, SpaceState tokens, params ITokenClass[] groups ) {
		int count = tokens.SumAny(groups) / 2; // half rounded down.
		pusher.AddGroup(count,groups);
	}


	[InnateTier( "2 sun,2 air", "Push up to 1 Invader/dahan/presence/beast." )]
	static public Task Option2( TargetSpaceCtx ctx ) {
		return ctx.Push(1, Human.Invader.Concat( ctx.AllPresenceTokens ).Plus( Human.Dahan ) );
	}

}
