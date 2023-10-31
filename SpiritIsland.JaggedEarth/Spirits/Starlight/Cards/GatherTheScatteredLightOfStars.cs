namespace SpiritIsland.JaggedEarth;

public class GatherTheScatteredLightOfStars {

	[SpiritCard("Gather the Scattered Light of Stars", 0, Element.Moon), Slow, Yourself]
	[Instructions( "At end of turn after discarding: Reclaim up to 2 cards to your hand. You may then Forget a Unique Power Card to Reclaim up to 3 additional cards." ), Artist( Artists.EmilyHancock )]
	public static async Task ActAsync( SelfCtx ctx ) { 

		// At end of turn after discarding: Reclaim up to 2 cards to your hand.
		int reclaimCount = 2;

		// You may then Forget a Unique Power Card 
		if( reclaimCount < ctx.Self.InPlay.Count + ctx.Self.DiscardPile.Count) {
			var uniques = ctx.Self.InPlay.Union( ctx.Self.Hand ).Union( ctx.Self.DiscardPile )
				.Where( x => x.PowerType == PowerType.Spirit )
				.ToList();
			var unique = await ctx.Self.SelectPowerCard( "Forget unique to reclaim 3 more.", uniques, CardUse.Forget, Present.Done );
			if(unique != null){
				ctx.Self.ForgetThisCard( unique );
				reclaimCount += 3;
			}
		}

		ctx.Self.AddActionFactory( new ReclaimN( reclaimCount ) );

	}

}