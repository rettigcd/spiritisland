using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland {

	public class Reclaim1InsteadOfDiscard {

		readonly Spirit spirit;
		readonly PowerCard[] purchased;

		public Reclaim1InsteadOfDiscard(Spirit spirit ) {
			this.spirit = spirit;
			this.purchased = spirit.PurchasedCards.ToArray(); // make copy in case spirit is cleaned up before this is called
		}

		public async Task Reclaim( GameState _ ) {
			var reclaimCard = (PowerCard)await spirit.Select( "Reclaim 1 played card", purchased.Cast<IActionFactory>().ToArray(), Present.Done );
			if(reclaimCard == null) return;

			spirit.Hand.Add( reclaimCard );
			spirit.DiscardPile.Remove( reclaimCard );
			spirit.PurchasedCards.Remove( reclaimCard );
		}

	}

}