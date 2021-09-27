using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SpiritIsland {

	public class Fear {

		readonly GameState gs;
		readonly int activationThreshold;
		public Fear(GameState gs ) {
			this.gs = gs;
			this.activationThreshold = gs.Spirits.Length * 4;
			gs.TimePassed += FearAdded.EndOfRound;
			Init();
		}

		public void Init() {
			while(Deck.Count < 9)
				AddCard( new NullFearCard() );
		}

		public void AddCard( IFearOptions fearCard ) {
			if(Deck.Count >= 9) throw new InvalidOperationException( "Fear deck is full." );
			var labels = new string[] { "1-A", "1-B", "1-C", "2-A", "2-B", "2-C", "3-A", "3-B", "3-C" };
			int index = 9 - Deck.Count - 1;
			var td = new PositionFearCard { FearOptions = fearCard, Text = "Lvl " + labels[index] };
			Deck.Push( td );
		}

		public int TerrorLevel {
			get {
				int ct = Deck.Count;
				int terrorLevel = ct > 6 ? 1 
					: ct > 3 ? 2 
					: 3;
				return terrorLevel;
			}
		}

		public void AddDirect( FearArgs args ) {
			Pool += args.count;
			while(activationThreshold <= Pool) { // should be while() - need unit test
				Pool -= activationThreshold;
				ActivatedCards.Push( Deck.Pop() );
				ActivatedCards.Peek().Text = "Active " + ActivatedCards.Count;
			}
			if(Deck.Count == 0)
				GameOverException.Win();
			FearAdded?.Invoke( gs, args );
		}

		public int Pool { get; private set; } = 0;
		public readonly Stack<PositionFearCard> Deck = new Stack<PositionFearCard>();
		public readonly Stack<PositionFearCard> ActivatedCards = new Stack<PositionFearCard>();

		public async Task Apply() {
			while(ActivatedCards.Count > 0) {
				PositionFearCard fearCard = ActivatedCards.Pop();
				// show card to each user
				foreach(var spirit in gs.Spirits)
					await spirit.ShowFearCardToUser( "Activating Fear", fearCard, TerrorLevel );

				var ctx = new FearCtx( gs );
				switch(TerrorLevel) {
					case 1: await fearCard.FearOptions.Level1( ctx ); break;
					case 2: await fearCard.FearOptions.Level2( ctx ); break;
					case 3: await fearCard.FearOptions.Level3( ctx ); break;
				}
			}
		}

		public SyncEvent<FearArgs> FearAdded = new SyncEvent<FearArgs>();                     // Dread Apparations

	}

	public class FearArgs {
		public int count;
		public Space space;
		public Cause cause;
	}

}
