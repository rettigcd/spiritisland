using System;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class WaryOfTheInterior : IFearCard {

		[FearLevel( 1, "Each player removes 1 Explorer from an Inland land." )]
		public Task Level1( GameState gs ) {
			return EachSpiritRemoves1Invader( gs, IsInland, Invader.Explorer );
		}

		[FearLevel( 2, "Each player removes 1 Explorer / Town from an Inland land." )]
		public Task Level2( GameState gs ) {
			return EachSpiritRemoves1Invader( gs, IsInland, Invader.Explorer, Invader.Town );
		}

		[FearLevel( 3, "Each player removes 1 Explorer / Town from any land." )]
		public Task Level3( GameState gs ) {
			return EachSpiritRemoves1Invader( gs, s=>true, Invader.Explorer, Invader.Town );
		}

		static bool IsInland(Space space) => !space.IsCostal;

		static async Task EachSpiritRemoves1Invader( GameState gs, Func<Space,bool> spaceCondition, params Invader[] x ) {
			foreach(var spirit in gs.Spirits) {
				var engine = spirit.Bind( gs );
				var options = gs.Island.AllSpaces
					.Where( spaceCondition )
					.Where( s => gs.InvadersOn( s ).HasAny( x ) )
					.ToArray();
				if(options.Length == 0) break;
				var target = await engine.SelectSpace( "Fear:select land to remove 1 explorer", options );
				var invaderToRemove = gs.InvadersOn( target ).PickBestInvaderToRemove( x );
				gs.Adjust( target, invaderToRemove, -1 );
			}
		}

	}
}
