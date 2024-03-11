namespace SpiritIsland.FeatherAndFlame;


[InnatePower( "Lay Paths They Cannot Help But Walk" ), Fast]
[FromPresence( 0 )]
[RepeatIf("2 moon,4 air,3 water")]
public class LayPathsTheyCannotHelpButWalk {

	[InnateTier( "2 moon,2 air", "Push up to half (rounded down) of Invaders from target land. Do likewise for dahan, presence, and beast (each separately).", 0 )]
	static async public Task Option1( TargetSpaceCtx ctx ) {

		var source = ctx.SourceSelector;

		// Push up to half( rounded down ) of Invaders from target land.
		AddHalf(source, ctx.Space, Human.Invader );
		// Do likewise for dahan
		AddHalf(source, ctx.Space, Human.Dahan);
		// Presence
		AddHalf( source, ctx.Space, ctx.AllPresenceTokens );
		// and beast( each separately ).
		AddHalf( source, ctx.Space, Token.Beast );

		await source.PushUpToN( ctx.Self );
	}

	static void AddHalf( SourceSelector ss, Space space, params ITokenClass[] groups ) {
		int count = space.SumAny(groups) / 2; // half rounded down.
		ss.AddGroup(count,groups);
	}


	[InnateTier( "2 sun,2 air", "Push up to 1 Invader/dahan/presence/beast.", 1 )]
	static public Task Option2( TargetSpaceCtx ctx ) {
		return ctx.Push(1, Human.Invader.Concat( ctx.AllPresenceTokens ).Plus( Human.Dahan ) );
	}

}
