﻿using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class Isolation : IFearOptions {

		public const string Name = "Isolation";

		[FearLevel( 1, "Each player removes 1 Explorer / Town from a land where it is the only Invader." )]
		public Task Level1( FearCtx ctx ) {
			var gs = ctx.GameState;
			return RemoveInvaderWhenMax(gs, 1, Invader.Explorer, Invader.Town );
		}

		[FearLevel( 2, "Each player removes 1 Explorer / Town from a land with 2 or fewer Invaders." )]
		public Task Level2( FearCtx ctx ) {
			var gs = ctx.GameState;
			return RemoveInvaderWhenMax( gs, 2, Invader.Explorer, Invader.Town );
		}

		[FearLevel( 3, "Each player removes an Invader from a land with 2 or fewer Invaders." )]
		public Task Level3( FearCtx ctx ) {
			var gs = ctx.GameState;
			return RemoveInvaderWhenMax( gs, 2, Invader.City, Invader.Explorer, Invader.Town );
		}

		/// <summary>
		/// Conditionally Removes an invader based on the Total # of invaders in a space
		/// </summary>
		static async Task RemoveInvaderWhenMax(GameState gs, int invaderMax, params TokenGroup[] removeableInvaders ) {
			foreach(var spirit in gs.Spirits) {
				var options = gs.Island.AllSpaces.Where( s => {
					var counts = gs.Tokens[ s ];
					return counts.HasAny( removeableInvaders ) && counts.InvaderTotal() <= invaderMax;
				} ).ToArray();
				if(options.Length == 0) return;

				var target = await spirit.Action.Decision( new Decision.TargetSpace( "fear:Select land to remove 1 explorer", options ));

				var grp = gs.Tokens[ target ];
				var invaderToRemove = grp.PickBestInvaderToRemove(removeableInvaders);
				grp.Adjust( invaderToRemove, -1 );
			}
		}
	}

}
