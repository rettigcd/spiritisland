namespace SpiritIsland.FeatherAndFlame;

public class ACircuitousAndWendingJourney {

	[SpiritCard( "A Circuitous and Wending Journey", 0, Element.Moon, Element.Air ),Slow,FromPresence( 0 )]
	[Instructions( "Push up to half (round up) of Invaders from target land. Do likewise (separately) for Dahan, Presence, and Beasts." ), Artist( Artists.MoroRogers )]
	static public Task ActAsync( TargetSpaceCtx ctx ) {

		var selector = ctx.SourceSelector;
		// Push up to half( round up ) of Invaders from target land.
		AddHalf( selector, ctx.Tokens, Human.Invader );
		// Do likewise( separately) for dahan, presence, and beast.
		AddHalf( selector, ctx.Tokens, Human.Dahan );
		AddHalf( selector, ctx.Tokens, ctx.AllPresenceTokens );
		AddHalf( selector, ctx.Tokens, Token.Beast );

		return selector.PushUpToN( ctx.Self );
	}

	static void AddHalf( SourceSelector source, SpaceState tokens, params ITokenClass[] groups ) {
		int count = (tokens.SumAny( groups )+1) / 2; // +1 causes rounds up
		source.AddGroup( count, groups );
	}

}

