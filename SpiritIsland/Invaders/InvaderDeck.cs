﻿using System;
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

		public static InvaderDeck BuildTestDeck( params InvaderCard[] cards ) => new InvaderDeck( cards );


		public static InvaderDeck Unshuffled() => new InvaderDeck((Random)null);

		#endregion

		#region constructors

		#region constructors

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
			// Setup draw: 1 card at a time.
			for(int i = 0; i < cards.Count; ++i) 
				drawCount.Add( 1 );

			InitExplorers(); // initialize the first explorer card up
		}

		#endregion

		#endregion

		public readonly List<InvaderCard> cards;
		public readonly List<int> drawCount = new List<int>(); // tracks how many cards to draw each turn

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

			InitExplorers();
		}

		void InitExplorers() {
			if(cards.Count > 0) {
				int count = drawCount[0]; drawCount.RemoveAt( 0 );
				while(count-- > 0) {
					Explore.Add( cards[0] );
					cards.RemoveAt( 0 );
				}
			}
		}

		public void DelayLastExploreCard() {
			if(drawCount.Count==0) drawCount.Add(0);

			var idx = Explore.Count - 1;
			var card = Explore[idx];
			Explore.RemoveAt( idx );
			cards.Insert( 0, card );
			drawCount[0]++;
		}

		#region Memento

		public virtual IMemento<InvaderDeck> SaveToMemento() => new Memento(this);
		public virtual void LoadFrom( IMemento<InvaderDeck> memento ) => ((Memento)memento).Restore(this);

		protected class Memento : IMemento<InvaderDeck> {
			public Memento(InvaderDeck src) {
				cards = src.cards.ToArray();
				drawCount = src.drawCount.ToArray();
			}
			public void Restore(InvaderDeck src ) {
				src.cards.SetItems(cards);
				src.drawCount.SetItems(drawCount);
			}
			readonly InvaderCard[] cards;
			readonly int[] drawCount;
		}

		#endregion

	}

}
