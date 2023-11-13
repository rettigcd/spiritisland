namespace SpiritIsland.FeatherAndFlame;

public class ACircuitousAndWendingJourney {

	[SpiritCard( "A Circuitous and Wending Journey", 0, Element.Moon, Element.Air ),Slow,FromPresence( 0 )]
	[Instructions( "Push up to half (round up) of Invaders from target land. Do likewise (separately) for Dahan, Presence, and Beasts." ), Artist( Artists.MoroRogers )]
	static public Task ActAsync( TargetSpaceCtx ctx ) {

		var pusher = ctx.Pusher;
		// Push up to half( round up ) of Invaders from target land.
		AddHalf( pusher, ctx.Tokens, Human.Invader );
		// Do likewise( separately) for dahan, presence, and beast.
		AddHalf( pusher, ctx.Tokens, Human.Dahan );
		AddHalf( pusher, ctx.Tokens, ctx.AllPresenceTokens );
		AddHalf( pusher, ctx.Tokens, Token.Beast );

		return pusher.DoUpToN();
	}

	static void AddHalf( TokenMover pusher, SpaceState tokens, params IEntityClass[] groups ) {
		int count = (tokens.SumAny( groups )+1) / 2; // +1 causes rounds up
		pusher.AddGroup( count, groups );
	}


}

