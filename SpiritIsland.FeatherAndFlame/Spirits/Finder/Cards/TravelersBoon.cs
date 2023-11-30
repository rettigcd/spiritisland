namespace SpiritIsland.FeatherAndFlame;

public class TravelersBoon {

	[SpiritCard( "Traveler's Boon", 0, Element.Moon, Element.Air, Element.Water ),Fast, AnotherSpirit]
	[Instructions( "Target spirit moves up to 3 of their Presence to one of your lands. They may move up to 1 Invader, 1 Dahan and 1 Beasts along with their Presence. (Total, not for each Presence.)" ), Artist( Artists.MoroRogers )]
	static public async Task ActAsync( TargetSpiritCtx ctx ) {

		// Target Spirit moves up to 3 of their presence to one of your lands.
		SelfCtx otherCtx = ctx.OtherCtx;

		// Select destination
		TargetSpaceCtx destinationCtx = await otherCtx.SelectTargetSpaceAsync("Move up to 3 of your presence to:", ctx.Self.Presence.Lands );
		// Select presence to pull in
		await new TokenMover(ctx.Self,"Move", otherCtx.Self.Presence.Lands.Tokens(), destinationCtx.Tokens)
			.AddGroup( 3, otherCtx.Self.Presence.Deployed.Select( x => x.Token.Class ).Distinct().ToArray() )
			// They may move up to 1 Invader, 1 dahan, and 1 beast along with their presence.
			// ( total, not for each presence).
			.Bring( Bring.FromAnywhere(ctx.Self,new Quota()
				.AddGroup( 1, Human.Invader )
				.AddGroup( 1, Human.Dahan )
				.AddGroup( 1, Token.Beast )
			))
			.DoN(Present.Done);

	}

}
