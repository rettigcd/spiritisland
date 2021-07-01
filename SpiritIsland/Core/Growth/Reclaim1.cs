using System;
using System.Linq;

namespace SpiritIsland.Core {

	public class Reclaim1 : GrowthActionFactory {

		public override IAction Bind( Spirit spirit, GameState gameState ) {
			return new Action(spirit);
		}

		class Action : IAction {

			PowerCard card;
			readonly Spirit spirit;

			public Action(Spirit spirit){
				this.spirit = spirit;
			}

			public bool IsResolved => card != null;

			public IOption[] Options => spirit.DiscardPile.Cast<IOption>().ToArray();

			public string Prompt => "Select card to reclaim.";

			public void Apply() {
				if( spirit.DiscardPile.Contains(card) ){
					spirit.DiscardPile.Remove(card);
					spirit.Hand.Add(card);
				}
			}

			public void Select( IOption option ) {
				card = (PowerCard)option;
			}
		}

	}

}
