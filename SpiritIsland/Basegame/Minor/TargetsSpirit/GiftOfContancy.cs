using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {
	public class GiftOfContancy {

		[MinorCard( "Gift of Contancy", 0, Speed.Fast, Element.Sun, Element.Earth )]
		[TargetSpirit]
		static public Task ActAsync( TargetSpiritCtx ctx ) {
			var target = ctx.Target;
			// target spirit gains 2 energy.  
			target.Energy += 2;

			// At end of turn, target spirit may reclaim 1 power card instead of discarding it.
			var purchased = target.PurchasedCards;
			ctx.GameState.TimePasses_ThisRound.Push( new Reclaim1InsteadOfDiscard(target).Reclaim );

			// if you target anouther spirit you may also reclaim 1 power Card instead of discarding it.
			if(target != ctx.Self)
				ctx.GameState.TimePasses_ThisRound.Push( new Reclaim1InsteadOfDiscard( ctx.Self ).Reclaim );

			return Task.CompletedTask;
		}

		class Reclaim1InsteadOfDiscard {
			readonly Spirit spirit;
			readonly PowerCard[] purchased;
			public Reclaim1InsteadOfDiscard(Spirit spirit ) {
				this.spirit = spirit;
				this.purchased = spirit.PurchasedCards.ToArray(); // make copy in case spirit is cleaned up before this is called
			}
			public async Task Reclaim(GameState _) {
				var reclaimCard = (PowerCard)await spirit.SelectFactory( "Gift of Contancy - Reclaim 1", purchased.Cast<IActionFactory>().ToArray(), true );
				if(reclaimCard != null) {
					spirit.Hand.Add( reclaimCard );
					spirit.DiscardPile.Remove( reclaimCard );
					spirit.PurchasedCards.Remove( reclaimCard );
				}
			}
		}

	}

}