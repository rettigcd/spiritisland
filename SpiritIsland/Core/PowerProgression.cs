using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland {

	// Plug into the DrawPowerCard API on the spirit
	public class PowerProgression : IPowerCardDrawer {
		readonly List<PowerCard> cards;
		public PowerProgression( params PowerCard[] cards ) {
			this.cards = cards.ToList();
		}

		public Task Draw( ActionEngine engine ) {
			return Take( engine.Self, cards.First() );
		}

		public Task DrawMajor( ActionEngine engine ) {
			return Take( engine.Self, cards.First( c => c.PowerType == PowerType.Major ) );
		}

		public Task DrawMinor( ActionEngine engine ) {
			return Take( engine.Self, cards.First( c => c.PowerType == PowerType.Minor ) );
		}

		Task Take( Spirit spirit, PowerCard newCard ) {
			cards.Remove( newCard );

			spirit.RegisterNewCard( newCard );
			if(newCard.PowerType == PowerType.Major)
				spirit.AddActionFactory( new ForgetPowerCard() ); // !!! do this right now, don't make it another factory
			return Task.CompletedTask;
		}

	}


}
