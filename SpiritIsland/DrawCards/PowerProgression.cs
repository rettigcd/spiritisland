using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland {

	// Plug into the DrawPowerCard API on the spirit
	public class PowerProgression : IPowerCardDrawer {
		public readonly PowerCard[] Cards;
		readonly List<PowerCard> remainingCards;
		public PowerProgression( params PowerCard[] cards ) {
			this.Cards = cards;
			this.remainingCards = cards.ToList();
		}

		public Task<DrawCardResult> Draw( Spirit self, GameState _ ) {
			return Take( self, remainingCards.First() );
		}

		public Task<DrawCardResult> DrawMajor( Spirit self, GameState _, int _2, int _3 ) {
			return Take( self, remainingCards.First( c => c.PowerType == PowerType.Major ) );
		}

		public Task<DrawCardResult> DrawMinor( Spirit self, GameState _, int _1, int _3 ) {
			return Take( self, remainingCards.First( c => c.PowerType == PowerType.Minor ) );
		}

		async Task<DrawCardResult> Take( Spirit self, PowerCard newCard ) {
			remainingCards.Remove( newCard );

			self.AddCardToHand( newCard );
			if(newCard.PowerType == PowerType.Major)
				await self.ForgetPowerCard();
			return new DrawCardResult( newCard.PowerType ) {
				SelectedCards = new PowerCard[]{ newCard },
				Rejected = new List<PowerCard>(),
			};
		}

	}


}
