namespace SpiritIsland.FeatherAndFlame;

public class OfferPassageBetweenWorlds {

	[SpiritCard( "Offer Passage Between Worlds", 1, Element.Sun, Element.Moon, Element.Air ),Fast,FromPresence( 1 )]
	[Instructions( "Move up to 4 Dahan between target land and one of your lands. -or- The next time Dahan would be Destroyed in target land, Destroy 2 fewer Dahan." ), Artist( Artists.MoroRogers )]
	static public Task ActAsync( TargetSpaceCtx ctx ) {

		return ctx.SelectActionOption(
			new SpaceCmd( "Move up to 4 dahan between target land and one of your lands.",
				xx => xx.MoveTokensToSingleLand( 4, new TargetCriteria( int.MaxValue, ctx.Self, Filter.Presence ), Human.Dahan )
			),
			Cmd.NextTimeDestroy2FewerDahan
		);
	}

}
