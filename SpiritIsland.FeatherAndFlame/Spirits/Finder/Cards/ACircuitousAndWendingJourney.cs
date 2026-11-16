namespace SpiritIsland.FeatherAndFlame;

public class ACircuitousAndWendingJourney {

	[SpiritCard( "A Circuitous and Wending Journey", 0, Element.Moon, Element.Air ),Slow,FromPresence( 0 )]
	[Instructions( "Push up to half (round up) of Invaders from target land. Do likewise (separately) for Dahan, Presence, and Beasts." ), Artist( Artists.MoroRogers )]
	static public Task ActAsync( TargetSpaceCtx ctx ) {

		// A selector takes one quota, so all 4 kinds are separate groups of that one quota.
		var quota = new Quota();
		// Push up to half( round up ) of Invaders from target land.
		AddHalf( quota, ctx.Space, Human.Invader );
		// Do likewise( separately) for dahan, presence, and beast.
		AddHalf( quota, ctx.Space, Human.Dahan );
		AddHalf( quota, ctx.Space, ctx.AllPresenceTokens );
		AddHalf( quota, ctx.Space, Token.Beast );

		return ctx.SourceSelector.UseQuota( quota ).PushUpToN( ctx.Self );
	}

	static void AddHalf( Quota quota, Space space, params ITokenClass[] groups ) {
		int count = (space.SumAny( groups )+1) / 2; // +1 causes rounds up
		quota.AddGroup( count, groups );
	}

}

