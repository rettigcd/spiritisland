﻿using SpiritIsland.Core;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class AvoidTheDahan : IFearCard {

		[FearLevel(1, "Invaders do not Explore into lands with at least 2 Dahan." )]
		public Task Level1(GameState gs) {
			gs.SkipExplore( gs.Island.AllSpaces.Where( s => gs.GetDahanOnSpace( s ) >= 2 ).ToArray() );
			return Task.CompletedTask;
		}

		[FearLevel( 2, "Invaders do not Build in lands where Dahan outnumber Town / City." )]
		public Task Level2( GameState gs ) {
			throw new System.NotImplementedException();
		}

		[FearLevel( 3, "Invaders do not Build in lands with Dahan." )]
		public Task Level3( GameState gs ) {
			throw new System.NotImplementedException();
		}

	}

}

