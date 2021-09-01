using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class Isolation : IFearCard {

		public const string Name = "Isolation";

		[FearLevel( 1, "Each player removes 1 Explorer / Town from a land where it is the only Invader." )]
		public Task Level1( GameState gs ) {
			return RemoveInvaderWhenMax(gs, 1, Invader.Explorer, Invader.Town );
		}

		[FearLevel( 2, "Each player removes 1 Explorer / Town from a land with 2 or fewer Invaders." )]
		public Task Level2( GameState gs ) {
			return RemoveInvaderWhenMax( gs, 2, Invader.Explorer, Invader.Town );
		}

		[FearLevel( 3, "Each player removes an Invader from a land with 2 or fewer Invaders." )]
		public Task Level3( GameState gs ) {
			return RemoveInvaderWhenMax( gs, 2, Invader.City, Invader.Explorer, Invader.Town );
		}

		/// <summary>
		/// Conditionally Removes an invader based on the Total # of invaders in a space
		/// </summary>
		static async Task RemoveInvaderWhenMax(GameState gs, int invaderMax, params Invader[] removeableInvaders ) {
			foreach(var spirit in gs.Spirits) {
				var options = gs.Island.AllSpaces.Where( s => {
					var grp = gs.Invaders.Counts[ s ];
					return grp.HasAny( removeableInvaders ) && grp.Total <= invaderMax;
				} ).ToArray();
				if(options.Length == 0) return;

				var target = await spirit.Action.Choose( new TargetSpaceDecision( "fear:Select land to remove 1 explorer", options ));

				var grp = gs.Invaders.Counts[ target ];
				var invaderToRemove = grp.PickBestInvaderToRemove(removeableInvaders);
				grp.Adjust( invaderToRemove, -1 );
			}
		}
	}

}
