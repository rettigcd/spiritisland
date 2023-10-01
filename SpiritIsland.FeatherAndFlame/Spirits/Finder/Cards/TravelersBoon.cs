namespace SpiritIsland.FeatherAndFlame;

public class TravelersBoon {

	[SpiritCard( "Traveler's Boon", 0, Element.Moon, Element.Air, Element.Water ),Fast, AnotherSpirit]
	[Instructions( "Target spirit moves up to 3 of their Presence to one of your lands. They may move up to 1 Invader, 1 Dahan and 1 Beasts along with their Presence. (Total, not for each Presence.)" ), Artist( Artists.MoroRogers )]
	static public async Task ActAsync( TargetSpiritCtx ctx ) {

		// Target Spirit moves up to 3 of their presence to one of your lands.
		var otherCtx = ctx.OtherCtx;
		var destinationCtx = await otherCtx.SelectSpace("Move up to 3 of your presence to:", ctx.Self.Presence.Spaces );
		var movedTokens = await new TokenCollectorFromSpecifiedSources( destinationCtx, otherCtx.Self.Presence.Spaces.Tokens().ToArray() )
			.AddGroup(3,otherCtx.Self.Presence.Deployed.Select(x=>x.Token.Class).Distinct().ToArray())
			.CollectUpToN();

		// They may move up to 1 Invader, 1 dahan, and 1 beast along with their presence.
		// ( total, not for each presence).
		if( 0 < movedTokens.Length)
		await new TokenCollectorFromSpecifiedSources( destinationCtx, movedTokens.Select( x => x.Space.Tokens ).Distinct().ToArray() )
			.AddGroup( 1, Human.Invader.Plus( Human.Dahan, Token.Beast ) )
			.CollectUpToN();

	}

}
