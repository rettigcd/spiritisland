using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class Isolation : IFearCard {

		[FearLevel( 1, "Each player removes 1 Explorer / Town from a land where it is the only Invader." )]
		public Task Level1( GameState gs ) {
			return RemoveInvaderWhenMax(gs,1, Invader.Explorer, Invader.Town );
		}

		[FearLevel( 2, "Each player removes 1 Explorer / Town from a land with 2 or fewer Invaders." )]
		public Task Level2( GameState gs ) {
			return RemoveInvaderWhenMax( gs, 2, Invader.Explorer, Invader.Town );
		}

		[FearLevel( 3, "Each player removes an Invader from a land with 2 or fewer Invaders." )]
		public Task Level3( GameState gs ) {
			return RemoveInvaderWhenMax( gs, 2, Invader.City, Invader.Explorer, Invader.Town );
		}

		static async Task RemoveInvaderWhenMax(GameState gs, int invaderMax,params Invader[] removeableInvaders ) {
			foreach(var spirit in gs.Spirits) {
				var options = gs.Island.AllSpaces.Where( s => {
					var grp = gs.InvadersOn( s );
					return grp.HasAny( removeableInvaders ) && grp.TotalCount <= invaderMax;
				} ).ToArray();
				if(options.Length == 0) return;

				var engine = new ActionEngine( spirit, gs );
				var target = await engine.SelectSpace( "fear:Select land to remove 1 explorer", options );

				var invaderToRemove = gs.InvadersOn(target).PickBestInvaderToRemove(removeableInvaders);
				gs.Adjust( target, invaderToRemove, -1 );
			}
		}
	}

}
