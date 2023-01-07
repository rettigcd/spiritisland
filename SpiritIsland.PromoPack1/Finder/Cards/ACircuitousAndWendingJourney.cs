namespace SpiritIsland.PromoPack1;

public class ACircuitousAndWendingJourney {

	[SpiritCard( "A Circuitous And Wending Journey", 0, Element.Moon, Element.Air )]
	[Slow,FromPresence( 0 )]
	static public Task ActAsync( TargetSpaceCtx ctx ) {

		var pusher = ctx.Pusher;
		// Push up to half( round up ) of Invaders from target land.
		AddHalf( pusher, ctx.Tokens, Invader.Any );
		// Do likewise( separately) for dahan, presence, and beast.
		AddHalf( pusher, ctx.Tokens, TokenType.Dahan );
		AddHalf( pusher, ctx.Tokens, ctx.AllPresenceTokens );
		AddHalf( pusher, ctx.Tokens, TokenType.Beast );

		return pusher.MoveUpToN();
	}

	static void AddHalf( TokenPusher pusher, SpaceState tokens, params TokenClass[] groups ) {
		int count = (tokens.SumAny( groups )+1) / 2; // +1 causes rounds up
		pusher.AddGroup( count, groups );
	}


}

