using System.Threading.Tasks;

namespace SpiritIsland.JaggedEarth {

	public class GiftOfTwinnedDays{ 
		[MinorCard("Gift of Twinned Days",1,Element.Sun,Element.Moon),Fast,AnotherSpirit]
		static public Task ActAsync( TargetSpiritCtx ctx ){
			// Once this turn, target spirit may repeat the lowest-cost Power Card they have in play by paying its cost again.
			ctx.Other.AddActionFactory(new RepeatCheapestCardForCost() );

			// You may do likewise.
			ctx.Self.AddActionFactory(new RepeatCheapestCardForCost() );

			return Task.CompletedTask;
		}
	}



}
