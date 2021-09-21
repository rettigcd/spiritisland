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
			new InvaderCard(Terrain.Jungle),
			new InvaderCard(Terrain.Wetland),
			new InvaderCard(Terrain.Sand),
			new InvaderCard(Terrain.Mountain),
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

		public static InvaderDeck Unshuffled() => new InvaderDeck(false);

		#endregion

		#region constructors

		public InvaderDeck(params InvaderCard[] cards ) {
			this.cards = cards.ToList();
			for(int i=0;i<cards.Count();++i) drawCount.Add(1);
			Advance(); // turn first explorer card up
		}

		public InvaderDeck():this(true){}

		InvaderDeck(bool shuffle){

			var level1 = Level1Cards.ToList();
			var level2 = Level2Cards.ToList();
			var level3 = Level3Cards.ToList();

			if(shuffle){
				level1.Shuffle();
				level2.Shuffle();
				level3.Shuffle(); 
			}

			static void Discard1(List<InvaderCard> list){ list.RemoveAt(list.Count-1); }
			Discard1(level1);
			Discard1(level2);
			Discard1(level3);

			// Merge
			var all = new List<InvaderCard>();
			all.AddRange(level1);
			all.AddRange(level2);
			all.AddRange(level3);
			cards = all.ToList();

			for(int i=0;i<cards.Count();++i) drawCount.Add(1);
			Advance(); // turn first explorer card up
		}

		#endregion

		public List<InvaderCard> Explore {get;} = new List<InvaderCard>();

		public List<InvaderCard> Build { get; } = new List<InvaderCard>();

		public List<InvaderCard> Ravage { get; } = new List<InvaderCard>();

		public int CountInDiscard {get; private set; }

		public void Advance(){
			// Move Ravage to Discard
			CountInDiscard += Ravage.Count;
			// Move Build to Ravage
			Ravage.Clear();
			Ravage.AddRange( Build );
			// move Explore to BUid
			Build.Clear();
			Build.AddRange( Explore );
			// turn over explore
			Explore.Clear();
			if(cards.Count > 0) {
				int count = drawCount[0]; drawCount.RemoveAt(0);
				while(count-- > 0) {
					Explore.Add( cards[0] );
					cards.RemoveAt(0);
				}
			}
		}

		readonly List<InvaderCard> cards;
		public readonly List<int> drawCount = new List<int>(); // tracks how many cards to draw each turn

	}

}
