using System;

namespace SpiritIsland.FeatherAndFlame;

public class OfferPassageBetweenWorlds {

	[SpiritCard( "Offer Passage Between Worlds", 1, Element.Sun, Element.Moon, Element.Air )]
	[Fast,FromPresence( 1 )]
	static public Task ActAsync( TargetSpaceCtx ctx ) {

		return ctx.SelectActionOption(
			new SpaceAction( "Move up to 4 dahan between target land and one of your lands.",
				xx => xx.MoveTokensOut( 4, new TargetCriteria( int.MaxValue, ctx.Self, Target.Presence ), Human.Dahan )
			),
			Cmd.NextTimeDestroy2FewerDahan
		);
	}

}
