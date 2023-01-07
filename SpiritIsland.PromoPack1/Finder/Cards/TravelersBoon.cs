namespace SpiritIsland.PromoPack1;

public class TravelersBoon {

	[SpiritCard( "Travelers Boon", 0, Element.Moon, Element.Air, Element.Water )]
	[Fast, AnotherSpirit]
	static public async Task ActAsync( TargetSpiritCtx ctx ) {

		// Target Spirit moves up to 3 of their presence to one of your lands.
		var otherCtx = ctx.OtherCtx;
		var destinationCtx = await otherCtx.SelectSpace("Move up to 3 of your presence to:", ctx.Presence.Spaces );
		var movedTokens = await new TokenCollectorFromSpecifiedSources( destinationCtx, otherCtx.Presence.SpaceStates.ToArray() )
			.AddGroup(3,otherCtx.Self.Presence.Token)
			.CollectUpToN();

		// They may move up to 1 Invader, 1 dahan, and 1 beast along with their presence.
		// ( total, not for each presence).
		if( 0 < movedTokens.Length)
		await new TokenCollectorFromSpecifiedSources( destinationCtx, movedTokens.Select( x => ctx.GameState.Tokens[x.Space] ).Distinct().ToArray() )
			.AddGroup( 1, Invader.Any.Plus( TokenType.Dahan, TokenType.Beast ) )
			.CollectUpToN();

	}

}
