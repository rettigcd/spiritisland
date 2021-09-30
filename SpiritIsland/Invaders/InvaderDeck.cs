using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace SpiritIsland {

	public class InvaderDeck {

		#region public static

		public static readonly ImmutableList<InvaderCard> Level1Cards = ImmutableList.Create(
			new InvaderCard(Terrain.Jungle),
			new InvaderCard(Terrain.Wetland),
			new InvaderCard(Terrain.Sand),
			new InvaderCard(Terrain.Mountain)
		);

		public static readonly ImmutableList<InvaderCard> Level2Cards = ImmutableList.Create(
			new InvaderCard(Terrain.Jungle, true),
			new InvaderCard(Terrain.Wetland, true),
			new InvaderCard(Terrain.Sand, true),
			new InvaderCard(Terrain.Mountain, true),
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

		public static InvaderDeck Unshuffled() => new InvaderDeck((Random)null);

		#endregion

		#region constructors

		static public InvaderDeck BuildTestDeck( params InvaderCard[] cards ) => new InvaderDeck( cards );

		private InvaderDeck( params InvaderCard[] cards ) {
			this.cards = cards.ToList();
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
			cards = all.ToList();

			Init();
		}

		void Init() {
			for(int i = 0; i < cards.Count; ++i) 
				drawCount.Add( 1 );
			TurnOverExploreCards(); // Advance(); // initialize the first explorer card up
		}

		#endregion

		public List<InvaderCard> Explore {get;} = new List<InvaderCard>();

		public List<InvaderCard> Build { get; } = new List<InvaderCard>();

		public List<InvaderCard> Ravage { get; } = new List<InvaderCard>();

		public int CountInDiscard {get; private set; }

		public bool KeepBuildCards = false;

		/// <summary>
		/// Triggers Ravage / 
		/// </summary>
		public void Advance() {
			// Move Ravage to Discard
			CountInDiscard += Ravage.Count;
			Ravage.Clear();
			// Move Build to Ravage
			if(KeepBuildCards)
				KeepBuildCards = false;
			else {
				Ravage.AddRange( Build );
				Build.Clear();
			}
			// move Explore to BUid
			Build.AddRange( Explore );
			Explore.Clear();

		}

		public void TurnOverExploreCards() {
			if(cards.Count > 0) {
				int count = drawCount[0]; drawCount.RemoveAt( 0 );
				while(count-- > 0) {
					Explore.Add( cards[0] );
					cards.RemoveAt( 0 );
				}
			}
		}

		readonly List<InvaderCard> cards;
		public readonly List<int> drawCount = new List<int>(); // tracks how many cards to draw each turn

	}

}
