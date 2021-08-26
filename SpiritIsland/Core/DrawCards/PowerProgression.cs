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

		public Task<PowerCard> Draw( Spirit self, GameState _, Func<List<PowerCard>, Task> _1 ) {
			return Take( self, cards.First() );
		}

		public Task<PowerCard> DrawMajor( Spirit self, GameState _, Func<List<PowerCard>, Task> _1 ) {
			return Take( self, cards.First( c => c.PowerType == PowerType.Major ) );
		}

		public Task<PowerCard> DrawMinor( Spirit self, GameState _, Func<List<PowerCard>, Task> _1 ) {
			return Take( self, cards.First( c => c.PowerType == PowerType.Minor ) );
		}

		async Task<PowerCard> Take( Spirit self, PowerCard newCard ) {
			cards.Remove( newCard );

			self.AddCardToHand( newCard );
			if(newCard.PowerType == PowerType.Major)
				await self.ForgetPowerCard();
			return newCard;
		}

	}


}
