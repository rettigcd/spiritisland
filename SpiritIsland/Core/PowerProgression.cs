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

		public Task<PowerCard> Draw( ActionEngine engine, Func<List<PowerCard>, Task> _ ) {
			return Take( engine, cards.First() );
		}

		public Task<PowerCard> DrawMajor( ActionEngine engine, Func<List<PowerCard>, Task> _ ) {
			return Take( engine, cards.First( c => c.PowerType == PowerType.Major ) );
		}

		public Task<PowerCard> DrawMinor( ActionEngine engine, Func<List<PowerCard>, Task> _ ) {
			return Take( engine, cards.First( c => c.PowerType == PowerType.Minor ) );
		}

		async Task<PowerCard> Take( ActionEngine engine, PowerCard newCard ) {
			var (spirit,_)=engine;
			cards.Remove( newCard );

			spirit.RegisterNewCard( newCard );
			if(newCard.PowerType == PowerType.Major)
				await spirit.ForgetPowerCard();
			return newCard;
		}

	}


}
