namespace SpiritIsland.FeatherAndFlame;

public class AidFromTheSpiritSpeakers {

	[SpiritCard( "Aid From the Spirit-Speakers", 2, Element.Sun, Element.Fire, Element.Air ),Fast, FromPresence( 1 )]
	[Instructions( "For each Dahan, you may move 1 Invader / Dahan / Presence / Beasts to a land within 2 Range that has Dahan." ), Artist( Artists.MoroRogers )]
	static public async Task ActAsync( TargetSpaceCtx ctx ) {

		// For each dahan, you may move 1 Invader / dahan / presence / beast to a land within range - 2 that has dahan.
		await ctx.MoveTokensToSingleLand( max: ctx.Dahan.CountAll
			, new TargetCriteria( 2, ctx.Self, Target.Dahan )
			, Human.Invader.Plus(Human.Dahan,Token.Beast)
				.Union( ctx.AllPresenceTokens )
				.ToArray()
			);

	}

}

