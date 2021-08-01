using System;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.Core {

	/// <summary>
	/// Replaces the DrawCard Growth Action when receiving a Major Power Progression Card.
	/// </summary>
	public class ForgetPowerCard : IActionFactory {

		public Speed Speed => Speed.Growth;

		public string Name => "Forget Power Card";

		public string Text => Name;

		public IActionFactory Original => this;

		public void Activate( ActionEngine engine ) {
			_ = ForgetCard(engine);
		}

		static async Task ForgetCard(ActionEngine engine ) {
			var self = engine.Self;
			var options = self.PurchasedCards.Union( self.Hand ).Union( self.DiscardPile )
				.ToArray();
			var cardToForget = await engine.SelectFactory("Select power card to forget", options);
			self.Forget( (PowerCard)cardToForget );
		}

	}

}
