﻿using System;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class EmigrationAccelerates : IFearCard {

		[FearLevel( 1, "Each player removes 1 Explorer from a Coastal land." )]
		public Task Level1( GameState gs ) {
			return ForEachSpiritSelectedLandRemoveInvader( gs, x => x.IsCostal, Invader.Explorer );
		}

		[FearLevel( 2, "Each player removes 1 Explorer / Town from a Coastal land." )]
		public Task Level2( GameState gs ) {
			return ForEachSpiritSelectedLandRemoveInvader( gs, x => x.IsCostal, Invader.Town, Invader.Explorer );
		}

		[FearLevel( 3, "Each player removes 1 Explorer / Town from any land." )]
		public Task Level3( GameState gs ) {
			return ForEachSpiritSelectedLandRemoveInvader( gs, x => true, Invader.Town, Invader.Explorer );
		}

		static async Task ForEachSpiritSelectedLandRemoveInvader( 
			GameState gs, 
			Func<Space, bool> landFilter, 
			params Invader[] removable
		) {
			foreach(var spirit in gs.Spirits) {
				var options = gs.Island.AllSpaces
					.Where( landFilter )
					.Where( x => gs.InvadersOn( x ).HasAny( removable ) )
					.ToArray();
				if(options.Length == 0) break;
				var target = await spirit.SelectSpace( "Fear:Pick costal land remove invader", options );
				var grp = gs.InvadersOn( target );

				var invaderToRemove = grp.PickBestInvaderToRemove( removable );
				gs.Adjust( target, invaderToRemove, -1 );
			}
		}

	}

}

