using System;
using System.Linq;

namespace SpiritIsland.Core {

	/// <summary>
	/// Replaces the DrawCard Growth Action when receiving a Major Power Progression Card.
	/// </summary>
	public class ForgetPowerCard : IActionFactory {

		public Speed Speed => Speed.Growth;

		public string Name => "Forget Power Card";

		public string Text => Name;

		public IActionFactory Original => this;

		public IAction Bind( Spirit spirit, GameState gameState ) {
			return new Action(spirit);
		}

		class Action : IAction {
			readonly Spirit self;
			public Action(Spirit self){ 
				this.self = self;
				this.Options = self.PurchasedCards.Union(self.Hand).Union(self.DiscardPile)
					.Cast<IOption>()
					.ToArray();
			}
			public string Prompt => "Select power card to forget";

			public IOption[] Options {get;}

			public bool IsResolved => cardToForget!=null;

			public string Selections => "n/a";

			PowerCard cardToForget = null;

			public void Select( IOption option ) {
				cardToForget = (PowerCard)option;
				self.Forget(cardToForget);
			}
		}

	}
}
