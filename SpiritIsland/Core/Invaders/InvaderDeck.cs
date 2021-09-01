﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace SpiritIsland {

	public class InvaderDeck {

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

		readonly InvaderCard[] cards;
		int exploreIndex;

		public static InvaderDeck Unshuffled() => new InvaderDeck(false);

		public InvaderDeck():this(true){}

		public InvaderDeck(params InvaderCard[] cards ) {
			this.cards = cards;
		}

		InvaderDeck(bool shuffle){

			var level1 = Level1Cards.ToList();
			var level2 = Level2Cards.ToList();
			var level3 = Level3Cards.ToList();

			if(shuffle){
				level1.Shuffle();
				level2.Shuffle();
				level3.Shuffle(); 
			}

			Discard1(level1);
			Discard1(level2);
			Discard1(level3);

			// Merge
			var all = new List<InvaderCard>();
			all.AddRange(level1);
			all.AddRange(level2);
			all.AddRange(level3);
			cards = all.ToArray();
		}

		static void Discard1(List<InvaderCard> list){
			list.RemoveAt(list.Count-1);
		}

		public InvaderCard Explore => cards[exploreIndex];
		public InvaderCard Build => 1<=exploreIndex ? cards[exploreIndex-1] : null;
		public InvaderCard Ravage => 2<=exploreIndex ? cards[exploreIndex-2] : null;

		public int CountInDiscard => exploreIndex <3 ? 0 : exploreIndex-2;

		public void Advance(){
			++exploreIndex;
		}

	}

}