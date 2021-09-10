using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public partial class GiftOfConstancy {

		[MinorCard( "Gift of Constancy", 0, Speed.Fast, Element.Sun, Element.Earth )]
		[TargetSpirit]
		static public Task ActAsync( TargetSpiritCtx ctx ) {

			// target spirit gains 2 energy.  
			ctx.Other.Energy += 2;

			// At end of turn, target spirit may reclaim 1 power card instead of discarding it.
			var purchased = ctx.Other.PurchasedCards;
			ctx.GameState.TimePasses_ThisRound.Push( new Reclaim1InsteadOfDiscard( ctx.Other ).Reclaim );

			// if you target another spirit you may also reclaim 1 power Card instead of discarding it.
			if(ctx.Other != ctx.Self)
				ctx.GameState.TimePasses_ThisRound.Push( new Reclaim1InsteadOfDiscard( ctx.Self ).Reclaim );

			return Task.CompletedTask;
		}

	}

}