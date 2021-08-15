using SpiritIsland;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {
	public class GiftOfContancy {

		[MinorCard( "Gift of Contancy", 0, Speed.Fast, Element.Sun, Element.Earth )]
		[TargetSpirit]
		static public Task ActAsync( ActionEngine engine, Spirit target ) {
			// target spirit gains 2 energy.  
			target.Energy += 2;

			// At end of turn, target spirit may reclaim 1 power card instead of discarding it.
			var purchased = target.PurchasedCards;
			engine.GameState.EndOfRoundCleanupAction.Push( new Reclaim1InsteadOfDiscard(target).Reclaim );

			// if you target anouther spirit you may also reclaim 1 power Card instead of discarding it.
			if(target != engine.Self)
				engine.GameState.EndOfRoundCleanupAction.Push( new Reclaim1InsteadOfDiscard( engine.Self ).Reclaim );

			return Task.CompletedTask;
		}

		class Reclaim1InsteadOfDiscard {
			readonly Spirit spirit;
			readonly PowerCard[] purchased;
			public Reclaim1InsteadOfDiscard(Spirit spirit ) {
				this.spirit = spirit;
				this.purchased = spirit.PurchasedCards.ToArray(); // make copy in case spirit is cleaned up before this is called
			}
			public async Task Reclaim(GameState gs ) {
				var reclaimCard = (PowerCard)await new ActionEngine( spirit, gs ).SelectFactory( "Gift of Contancy - Reclaim 1", purchased.Cast<IActionFactory>().ToArray(), true );
				if(reclaimCard != null) {
					spirit.Hand.Add( reclaimCard );
					spirit.DiscardPile.Remove( reclaimCard );
					spirit.PurchasedCards.Remove( reclaimCard );
				}
			}
		}

	}

}