using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace SpiritIsland {

	public class InvaderDeck {

		#region public static

		public static readonly ImmutableList<InvaderCard> Level1Cards = ImmutableList.Create(
			new InvaderCard( Terrain.Jungle ),
			new InvaderCard( Terrain.Wetland ),
			new InvaderCard( Terrain.Sand ),
			new InvaderCard( Terrain.Mountain )
		);

		public static readonly ImmutableList<InvaderCard> Level2Cards = ImmutableList.Create(
			new InvaderCard( Terrain.Jungle, true),
			new InvaderCard( Terrain.Wetland, true),
			new InvaderCard( Terrain.Sand, true),
			new InvaderCard( Terrain.Mountain, true),
			InvaderCard.Costal
		);

		public static readonly ImmutableList<InvaderCard> Level3Cards = ImmutableList.Create(
			new InvaderCard(Terrain.Jungle,Terrain.Sand),
			new InvaderCard(Terrain.Jungle,Terrain.Mountain),
			new InvaderCard(Terrain.Jungle,Terrain.Wetland),
			new InvaderCard(Terrain.Mountain,Terrain.Sand),
			new InvaderCard(Terrain.Mountain,Terrain.Wetland),
			new InvaderCard(Terrain.Sand,Terrain.Wetland)
		);

		public static InvaderDeck BuildTestDeck( params InvaderCard[] cards ) => new InvaderDeck( cards );


		public static InvaderDeck Unshuffled() => new InvaderDeck((Random)null);

		#endregion

		#region constructors

		#region constructors

		private InvaderDeck( params InvaderCard[] cards ) {
			this.unrevealedCards = cards.ToList();
			Init();
		}

		public InvaderDeck( Random random ) {

			var level1 = Level1Cards.ToList();
			var level2 = Level2Cards.ToList();
			var level3 = Level3Cards.ToList();

			if(random != null) {
				random.Shuffle( level1 );
				random.Shuffle( level2 );
				random.Shuffle( level3 );
			}

			static void DiscardLast( List<InvaderCard> list ) { list.RemoveAt( list.Count - 1 ); }
			DiscardLast( level1 );
			DiscardLast( level2 );
			DiscardLast( level3 );

			// Merge
			var all = new List<InvaderCard>();
			all.AddRange( level1 );
			all.AddRange( level2 );
			all.AddRange( level3 );
			unrevealedCards = all.ToList();

			Init();
		}

		void Init() {
			// Setup draw: 1 card at a time.
			for(int i = 0; i < unrevealedCards.Count; ++i) 
				drawCount.Add( 1 );

			InitExplorers(); // initialize the first explorer card up
		}

		#endregion

		#endregion

		public readonly List<InvaderCard> unrevealedCards;
		public readonly List<int> drawCount = new List<int>(); // tracks how many cards to draw each turn

		public List<InvaderCard> Explore {get;} = new List<InvaderCard>();

		public List<InvaderCard> Build { get; } = new List<InvaderCard>();

		public List<InvaderCard> Ravage { get; } = new List<InvaderCard>();

		public int CountInDiscard => Discards.Count;
		public List<InvaderCard> Discards {get;} = new List<InvaderCard>();


		public bool KeepBuildCards = false; // !!! is there a way to make this go away?


		/// <summary>
		/// Triggers Ravage / 
		/// </summary>
		public void Advance() {

			// Move Ravage to Discard
			Discards.AddRange( Ravage );
			Ravage.Clear();

			// Move Build to Ravage
			if(KeepBuildCards)
				KeepBuildCards = false;
			else {
				Ravage.AddRange( Build );
				Build.Clear();
			}

			// move Explore to Build
			CheckIfTimeRunsOut();
			Build.AddRange( Explore );
			Explore.Clear();

			InitExplorers();
		}

		void CheckIfTimeRunsOut() {
			if( Explore.Count==0 && unrevealedCards.Count==0 )
				GameOverException.Lost("Time runs out");
		}

		void InitExplorers() {
			if(unrevealedCards.Count > 0) {
				int count = drawCount[0]; drawCount.RemoveAt( 0 );
				while(count-- > 0) {
					Explore.Add( unrevealedCards[0] );
					unrevealedCards.RemoveAt( 0 );
				}
			}
		}

		public void DelayLastExploreCard() {
			if(drawCount.Count==0) drawCount.Add(0);

			var idx = Explore.Count - 1;
			var card = Explore[idx];
			Explore.RemoveAt( idx );
			unrevealedCards.Insert( 0, card );
			drawCount[0]++;
		}

		#region Memento

		public virtual IMemento<InvaderDeck> SaveToMemento() => new Memento(this);
		public virtual void LoadFrom( IMemento<InvaderDeck> memento ) => ((Memento)memento).Restore(this);

		protected class Memento : IMemento<InvaderDeck> {
			public Memento(InvaderDeck src) {
				unrevealedCards = src.unrevealedCards.ToArray();
				drawCount = src.drawCount.ToArray();

				explore = src.Explore.ToArray();
				build = src.Build.ToArray();
				ravage = src.Ravage.ToArray();
				discards = src.Discards.ToArray();

			}
			public void Restore(InvaderDeck src ) {
				src.unrevealedCards.SetItems(unrevealedCards);
				src.drawCount.SetItems(drawCount);
				src.Explore.SetItems(explore);
				src.Build.SetItems(build);
				src.Ravage.SetItems(ravage);
				src.Discards.SetItems(discards);
			}
			readonly InvaderCard[] unrevealedCards;
			readonly int[] drawCount;
			readonly InvaderCard[] explore;
			readonly InvaderCard[] build;
			readonly InvaderCard[] ravage;
			readonly InvaderCard[] discards;

		}

		#endregion

	}

}
