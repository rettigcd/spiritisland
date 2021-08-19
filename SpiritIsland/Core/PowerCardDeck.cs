using System.Collections.Generic;
using System.Linq;

namespace SpiritIsland {
	public class PowerCardDeck {

		public PowerCardDeck(IList<PowerCard> cards ) {
			discards = cards.ToList();
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
			discards.Shuffle();
			foreach(var card in discards) cards.Push(card);
			discards.Clear();
		}

		readonly Stack<PowerCard> cards = new Stack<PowerCard>();
		readonly List<PowerCard> discards;

	}
}
