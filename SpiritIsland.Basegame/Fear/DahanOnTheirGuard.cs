﻿using System;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class DahanOnTheirGuard : IFearOptions {
		public const string Name = "Dahan on their Guard";

		[FearLevel( 1, "In each land, Defend 1 per Dahan." )]
		public Task Level1( FearCtx ctx ) {
			var gs = ctx.GameState;
			int defend( Space space ) => gs.DahanGetCount( space );
			return DefendIt( gs, defend );
		}

		// "In each land with Dahan, Defend 1, plus an additional Defend 1 per Dahan.", 
		[FearLevel( 2, "" )]
		public Task Level2( FearCtx ctx ) {
			var gs = ctx.GameState;
			int defend(Space space) => 1 + gs.DahanGetCount( space );
			return DefendIt( gs, defend );
		}

		// "In each land, Defend 2 per Dahan."),
		[FearLevel( 3, "" )]
		Task IFearOptions.Level3( FearCtx ctx ) {
			var gs = ctx.GameState;
			int defend( Space space ) => 2* gs.DahanGetCount( space );
			return DefendIt( gs, defend );
		}

		static Task DefendIt( GameState gs, Func<Space, int> d ) {
			foreach(var space in gs.Island.AllSpaces.Where( gs.DahanIsOn ))
				gs.Tokens[space].Defend.Count += d( space );
			return Task.CompletedTask;
		}
	}

}
