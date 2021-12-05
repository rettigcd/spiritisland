using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.JaggedEarth {

	public class GatherTheScatteredLightOfStars {

		[SpiritCard("Gather the Scattered Light of Stars", 0, Element.Moon), Slow, Yourself]
		public static async Task ActAsync(TargetSpiritCtx ctx ) { 

			// At end of turn after discarding: Reclaim up to 2 cards to your hand.
			int reclaimCount = 2;

			// You may then Forget a Unique Power Card 
			if( reclaimCount < ctx.Self.InPlay.Count + ctx.Self.DiscardPile.Count) {
				var uniques = ctx.Self.InPlay.Union( ctx.Self.Hand ).Union( ctx.Self.DiscardPile )
					.Where( x => x.PowerType == PowerType.Spirit )
					.ToList();
				var unique = await ctx.Self.SelectPowerCard( "Forget unique to reclaim 3 more.", uniques, CardUse.Forget, Present.Done );
				if(unique != null){
					ctx.Self.Forget( unique ); // !!! does this create a null?
					reclaimCount += 3;
				}
			}

			ctx.GameState.TimePasses_ThisRound.Push( async (gs) => {
				for(int i=0;i<reclaimCount;++i)
					await ctx.Self.Reclaim1FromDiscard();
			} );

		}

	}

}
