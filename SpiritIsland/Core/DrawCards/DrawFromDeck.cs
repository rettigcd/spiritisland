using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SpiritIsland {

	public class DrawFromDeck : IPowerCardDrawer {

		public async Task<PowerCard> Draw( Spirit spirit, GameState gs, Func<List<PowerCard>, Task> handleNotUsed ) {
			var deck = await spirit.UserSelectsFirstText( "Which type do you wish to draw", "minor", "major" )
				? gs.MinorCards
				: gs.MajorCards;
			return await DrawInner( spirit, deck, handleNotUsed );
		}

		public Task<PowerCard> DrawMajor( Spirit spirit, GameState gameState, Func<List<PowerCard>, Task> handleNotUsed )
			=> DrawInner(spirit, gameState.MajorCards, handleNotUsed );
		

		public Task<PowerCard> DrawMinor( Spirit spirit, GameState gameState, Func<List<PowerCard>, Task> handleNotUsed )
			=> DrawInner( spirit, gameState.MinorCards, handleNotUsed );

		static async Task<PowerCard> DrawInner( Spirit spirit, PowerCardDeck deck, Func<List<PowerCard>, Task> handleNotUsed ) {
			List<PowerCard> flipped = deck.Flip(4);

			var selected = await TakeCard( spirit, flipped );

			if(handleNotUsed != null)
				await handleNotUsed( flipped );

			deck.Discard( flipped );
			return selected;
		}

		public static async Task<PowerCard> TakeCard( Spirit spirit, List<PowerCard> flipped ) {
			var selectedCard = (PowerCard)await spirit.SelectFactory( "Select new Power Card", flipped.ToArray() );
			spirit.Hand.Add( selectedCard );
			flipped.Remove( selectedCard );
			return selectedCard;
		}

	}


}