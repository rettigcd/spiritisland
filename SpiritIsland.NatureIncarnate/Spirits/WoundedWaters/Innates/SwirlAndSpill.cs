namespace SpiritIsland.NatureIncarnate;

// fast, range 0, ANY
// 2 moon, 2 air Push up to half (rounded down) of Invaders from target land.  Do likewise for Dahan, Presence, and Beast (each separately).
// 2 sun, 2 air Push up to 1 Invader/Dahan/Presence/Beast.
// 2 moon, 4 air, 3 water Repeat this Power


[InnatePower( "Swirl and Spill" ), Slow]
[FromPresence( 1 )]
public class SwirlAndSpill {

	[InnateOption( "2 water", "Push up to 2 Explorer/Dahan/Blight.", 0 )]
	static public async Task Option1( TargetSpaceCtx ctx ) {
		await ctx.PushUpTo(2,Human.Explorer,Human.Dahan,Token.Blight);
	}

	[InnateOption( "3 water,1 animal", "1 Fear. Push up to 2 Town/Presence/Beast.", 0 )]
	static public async Task Option2( TargetSpaceCtx ctx ) {
		await DoTier2( ctx );
	}

	// Does tier-2 and returns spaces pushed to
	static async Task<Space[]> DoTier2( TargetSpaceCtx ctx ) {
		ctx.AddFear( 1 );

		var destinations = await ctx.Pusher
			.AddGroup( 2, Human.Explorer, Human.Dahan, Token.Blight ) // from Tier 1
			.AddGroup( 2, Human.Town, ctx.Self.Presence.Token, Token.Beast ) // Tier 2
			.MoveUpToN();
		return destinations;
	}

	[InnateOption( "5 water,2 plant,2 animal", "In one land pushed into, Downgrade all Town and all City." )]
	static public async Task Option3( TargetSpaceCtx ctx ) {
		Space[] destinations = await DoTier2( ctx );

		IEnumerable<SpaceState> options = destinations.Tokens().Where( t => t.HasAny( Human.Town_City ) ).Distinct();
		Space spaceToDownGrade = await ctx.Self.Gateway.Decision( new Select.ASpace( "Select space to downgrade all Towns/Cities.", options, Present.Done ) );
		if(spaceToDownGrade == null) return;

		await ReplaceInvader.DowngradeAll( ctx, Human.Town_City );
	}

}
