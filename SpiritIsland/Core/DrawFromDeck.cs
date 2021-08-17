using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SpiritIsland {

	public class DrawFromDeck : IPowerCardDrawer {

		public async Task Draw( ActionEngine engine, Func<List<PowerCard>, Task> handleNotUsed ) {
			var deck = await engine.SelectFirstText( "Which type do you wish to draw", "minor", "major" )
				? engine.GameState.MinorCards
				: engine.GameState.MajorCards;
			await DrawInner( engine, deck, handleNotUsed );
		}

		public Task DrawMajor( ActionEngine engine, Func<List<PowerCard>, Task> handleNotUsed )
			=> DrawInner(engine, engine.GameState.MajorCards, handleNotUsed );
		

		public Task DrawMinor( ActionEngine engine, Func<List<PowerCard>, Task> handleNotUsed )
			=> DrawInner( engine, engine.GameState.MinorCards, handleNotUsed );

		static async Task DrawInner( ActionEngine engine, PowerCardDeck deck, Func<List<PowerCard>, Task> handleNotUsed ) {
			List<PowerCard> flipped = deck.Flip(4);

			await TakeCard( engine, flipped );

			if(handleNotUsed != null)
				await handleNotUsed( flipped );

			deck.Discard( flipped );
		}

		public static async Task TakeCard( ActionEngine engine, List<PowerCard> flipped ) {
			var selectedCard = (PowerCard)await engine.SelectFactory( "Select new Power Card", flipped.ToArray() );
			engine.Self.Hand.Add( selectedCard );
			flipped.Remove( selectedCard );
		}

	}


}