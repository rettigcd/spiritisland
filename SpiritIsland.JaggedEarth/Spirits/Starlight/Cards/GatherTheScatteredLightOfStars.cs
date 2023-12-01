namespace SpiritIsland.JaggedEarth;

public class GatherTheScatteredLightOfStars {

	[SpiritCard("Gather the Scattered Light of Stars", 0, Element.Moon), Slow, Yourself]
	[Instructions( "At end of turn after discarding: Reclaim up to 2 cards to your hand. You may then Forget a Unique Power Card to Reclaim up to 3 additional cards." ), Artist( Artists.EmilyHancock )]
	public static async Task ActAsync( Spirit self ) { 

		// At end of turn after discarding: Reclaim up to 2 cards to your hand.
		int reclaimCount = 2;

		// You may then Forget a Unique Power Card 
		if( reclaimCount < self.InPlay.Count + self.DiscardPile.Count) {
			var uniques = self.InPlay.Union( self.Hand ).Union( self.DiscardPile )
				.Where( x => x.PowerType == PowerType.Spirit )
				.ToList();
			var unique = await self.SelectPowerCard( "Forget unique to reclaim 3 more.", uniques, CardUse.Forget, Present.Done );
			if(unique != null){
				self.ForgetThisCard( unique );
				reclaimCount += 3;
			}
		}

		self.AddActionFactory( new SpiritGrowthAction( new ReclaimN( reclaimCount ) ) );

	}

}