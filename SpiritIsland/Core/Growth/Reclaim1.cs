using System;
using System.Linq;

namespace SpiritIsland.Core {

	public class Reclaim1 : GrowthActionFactory {

		public override IAction Bind( Spirit spirit, GameState gameState ) {
			return new Action(spirit);
		}

		public override string ShortDescription => "Reclaim(1)";

		class Action : IAction {

			PowerCard card;
			readonly Spirit spirit;

			public Action(Spirit spirit){
				this.spirit = spirit;
				Selections = "";
			}

			public bool IsResolved => Options.Length==0 || card != null;

			public IOption[] Options => spirit.DiscardPile.Cast<IOption>().ToArray();

			public string Prompt => "Select card to reclaim.";

			public string Selections { get; private set; }

			public void Select( IOption option ) {
				card = (PowerCard)option;
				Selections = Prompt + ":" + option.Text;

				if( card!=null && spirit.DiscardPile.Contains(card) ){
					spirit.DiscardPile.Remove(card);
					spirit.Hand.Add(card);
				}

			}
		}

	}

}
