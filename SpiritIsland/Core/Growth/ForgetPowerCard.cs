using System;
using System.Linq;

namespace SpiritIsland.Core {

	public class ForgetPowerCard : IActionFactory {
		public Speed Speed => Speed.Growth;

		public string Name => "Forget Power Card";

		public string Text => Name;

		public IAction Bind( Spirit spirit, GameState gameState ) {
			throw new NotImplementedException();
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

			public void Apply() {
				self.Forget(cardToForget);
			}
			PowerCard cardToForget = null;

			public void Select( IOption option ) {
				cardToForget = (PowerCard)option;
			}
		}

	}
}
