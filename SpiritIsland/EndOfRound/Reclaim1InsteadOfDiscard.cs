using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland {

	public class Reclaim1InsteadOfDiscard {

		readonly Spirit spirit;
		readonly PowerCard[] inPlay;

		public Reclaim1InsteadOfDiscard(Spirit spirit ) {
			this.spirit = spirit;
			this.inPlay = spirit.InPlay.ToArray(); // make copy in case spirit is cleaned up before this is called
		}

		public async Task Reclaim( GameState _ ) {
			var reclaimCard = await spirit.SelectPowerCard( "Reclaim 1 played card", inPlay, CardUse.Reclaim, Present.Done );
			if(reclaimCard == null) return;

			// !!! replace with spirit.Reclaim( reclaimCard );
			spirit.Hand.Add( reclaimCard );
			spirit.DiscardPile.Remove( reclaimCard );
			spirit.InPlay.Remove( reclaimCard );
		}

	}

}