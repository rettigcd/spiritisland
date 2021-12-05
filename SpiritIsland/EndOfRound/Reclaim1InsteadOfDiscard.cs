using System.Threading.Tasks;

namespace SpiritIsland {

	public class Reclaim1InsteadOfDiscard {

		readonly Spirit spirit;
		readonly PowerCard[] purchased;

		public Reclaim1InsteadOfDiscard(Spirit spirit ) {
			this.spirit = spirit;
			this.purchased = spirit.InPlay.ToArray(); // make copy in case spirit is cleaned up before this is called
		}

		public async Task Reclaim( GameState _ ) {
			var reclaimCard = await spirit.SelectPowerCard( "Reclaim 1 played card", purchased, CardUse.Reclaim, Present.Done );
			if(reclaimCard != null)
				spirit.Reclaim(reclaimCard);
		}

	}

}