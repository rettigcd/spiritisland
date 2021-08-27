using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SpiritIsland {

	public class Fear {

		readonly GameState gs;
		public Fear(GameState gs ) {
			this.gs = gs;
			gs.TimePassed += ClearOneTurnHandlers;
			Init();
		}

		public void Init() {
			while(Deck.Count < 9)
				AddCard( new NullFearCard() );
		}

		public void AddCard( IFearCard fearCard ) {
			if(Deck.Count >= 9) throw new InvalidOperationException( "Fear deck is full." );
			var labels = new string[] { "1-A", "1-B", "1-C", "2-A", "2-B", "2-C", "3-A", "3-B", "3-C" };
			int index = 9 - Deck.Count - 1;
			var td = new NamedFearCard { Card = fearCard, Text = "Lvl " + labels[index] };
			Deck.Push( td );
		}

		public int TerrorLevel {
			get {
				int ct = Deck.Count;
				int terrorLevel = ct > 6 ? 1 : ct > 3 ? 2 : 1;
				return terrorLevel;
			}
		}

		public void AddDirect( FearArgs args ) {
			Pool += args.count;
			if(4 <= Pool) { // should be while() - need unit test
				Pool -= 4;
				ActivatedCards.Push( Deck.Pop() );
				ActivatedCards.Peek().Text = "Active " + ActivatedCards.Count;
			}
			if(Deck.Count == 0)
				GameOverException.Win();
			Added_ThisRound?.Invoke( gs, args );
		}

		public int Pool { get; private set; } = 0;
		public readonly Stack<NamedFearCard> Deck = new Stack<NamedFearCard>();
		public readonly Stack<NamedFearCard> ActivatedCards = new Stack<NamedFearCard>();

		public SyncEvent<FearArgs> Added_ThisRound = new SyncEvent<FearArgs>();                     // Dread Apparations

		public async Task Apply() {
			while(ActivatedCards.Count > 0) {
				NamedFearCard fearCard = ActivatedCards.Pop();
				// show card to each user
				foreach(var spirit in gs.Spirits)
					await spirit.ShowFearCardToUser( "Activating Fear", fearCard );

				switch(TerrorLevel) {
					case 1: await fearCard.Card.Level1( gs ); break;
					case 2: await fearCard.Card.Level2( gs ); break;
					case 3: await fearCard.Card.Level3( gs ); break;
				}
			}
		}

		void ClearOneTurnHandlers( GameState obj ) {
			Added_ThisRound.Handlers.Clear();
		}

	}

	public class FearArgs {
		public int count;
		public Space space;
		public Cause cause;
	}

}
