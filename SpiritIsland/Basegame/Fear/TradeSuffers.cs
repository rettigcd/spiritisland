using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class TradeSuffers : IFearCard {

		[FearLevel( 1, "Invaders do not Build in lands with City." )]
		public Task Level1( GameState gs ) {
			gs.SkipBuild( gs.Island.AllSpaces.Where( s => gs.InvadersOn( s ).HasCity ).ToArray() );
			// !! no unit tests on this
			return Task.CompletedTask;
		}

		[FearLevel( 2, "Each player may replace 1 Town with 1 Explorer in a Coastal land." )]
		public async Task Level2( GameState gs ) {
			foreach(var spirit in gs.Spirits) {
				var options = gs.Island.AllSpaces.Where(s=>s.IsCostal&&gs.InvadersOn(s).HasTown).ToArray();
				if(options.Length==0) return;
				var target = await spirit.SelectSpace("Replace town with explorer",options);
				gs.Adjust(target,InvaderSpecific.Town,-1);
				gs.Adjust( target, InvaderSpecific.Explorer, 1 );
			}
		}

		[FearLevel( 3, "Each player may replace 1 City with 1 Town or 1 Town with 1 Explorer in a Coastal land." )]
		public async Task Level3( GameState gs ) {
			foreach(var spirit in gs.Spirits) {
				var options = gs.Island.AllSpaces.Where( s => s.IsCostal && gs.InvadersOn( s ).HasAny(Invader.Town,Invader.City) ).ToArray();
				if(options.Length == 0) return;
				var target = await spirit.SelectSpace( "Replace town with explorer", options );
				if(gs.InvadersOn( target ).HasCity) {
					gs.Adjust( target, InvaderSpecific.City, -1 );
					gs.Adjust( target, InvaderSpecific.Town, 1 );
				} else {
					gs.Adjust( target, InvaderSpecific.Town, -1 );
					gs.Adjust( target, InvaderSpecific.Explorer, 1 );
				}
			}
		}

	}

}
