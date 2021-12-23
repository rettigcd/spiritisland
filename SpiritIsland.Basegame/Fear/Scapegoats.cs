﻿using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class Scapegoats : IFearOptions {

		public const string Name = "Scapegoats";

		[FearLevel( 1, "Each Town destroys 1 Explorer in its land." )]
		public Task Level1( FearCtx ctx ) {
			var gs = ctx.GameState;
			foreach(var space in gs.Island.AllSpaces)
				Destroy_1ExplorerPerTown( gs.Invaders.On( space ) );
			return Task.CompletedTask;
		}

		[FearLevel( 2, "Each Town destroys 1 Explorer in its land. Each City destroys 2 Explorer in its land." )]
		public Task Level2( FearCtx ctx ) {
			var gs = ctx.GameState;
			foreach(var space in gs.Island.AllSpaces) {
				var grp = gs.Invaders.On( space );
				Destory_1ExplorerPerTownAnd2ExplorersPerCity( grp );
			}
			return Task.CompletedTask;
		}

		[FearLevel( 3, "Destroy all Explorer in lands with Town / City. Each City destroys 1 Town in its land." )]
		public async Task Level3( FearCtx ctx ) {
			var gs = ctx.GameState;
			foreach(var space in gs.Island.AllSpaces) {
				var grp = (InvaderBinding)gs.Invaders.On( space );
				await grp.Destroy( int.MaxValue, Invader.Explorer );
				await EachCityDestroys1Town( grp );
		
			}
		}

		static Task Destroy_1ExplorerPerTown( InvaderBinding grp ) {
			return grp.Destroy( grp.Tokens.Sum( Invader.Town ), Invader.Explorer );
		}

		static Task EachCityDestroys1Town( InvaderBinding grp ) {
			return grp.Destroy( grp.Tokens.Sum( Invader.City ), Invader.Town );
		}

		static Task Destory_1ExplorerPerTownAnd2ExplorersPerCity( InvaderBinding grp ) {
			int numToDestory = grp.Tokens.Sum(Invader.Town) + grp.Tokens.Sum(Invader.City) * 2;
			return grp.Destroy( numToDestory, Invader.Explorer );
		}


	}
}

