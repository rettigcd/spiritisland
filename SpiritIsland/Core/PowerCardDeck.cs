using System;
using System.Collections.Generic;
using System.Linq;

namespace SpiritIsland {

	public class PowerCardDeck {

		readonly Random randomizer;

		public PowerCardDeck(IList<PowerCard> cards, Random randomizer) {
			discards = cards.ToList();
			this.randomizer = randomizer;
		}

		public PowerCard FlipNext() {
			if(cards.Count == 0)
				ReshuffleDiscardDeck();
			var next = cards.Pop();
			return next;
		}

		public List<PowerCard> Flip( int count ) {
			var flipped = new List<PowerCard>();
			for(int i = 0; i < count; ++i) flipped.Add( FlipNext() );
			return flipped;
		}

		public void Discard(IEnumerable<PowerCard> discards) => this.discards.AddRange(discards);

		void ReshuffleDiscardDeck() {
			randomizer.Shuffle(discards);
			foreach(var card in discards) cards.Push(card);
			discards.Clear();
		}

		readonly Stack<PowerCard> cards = new Stack<PowerCard>();
		readonly List<PowerCard> discards;

	}
}
