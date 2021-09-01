using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class TradeSuffers : IFearCard {

		public const string Name = "Trade Suffers";

		[FearLevel( 1, "Invaders do not Build in lands with City." )]
		public Task Level1( GameState gs ) {
			gs.SkipBuild( gs.Island.AllSpaces.Where( s => gs.Invaders.Counts[ s ].Has(Invader.City) ).ToArray() );
			// !! no unit tests on this
			return Task.CompletedTask;
		}

		[FearLevel( 2, "Each player may replace 1 Town with 1 Explorer in a Coastal land." )]
		public async Task Level2( GameState gs ) {
			foreach(var spirit in gs.Spirits) {
				var options = gs.Island.AllSpaces.Where(s=>s.IsCostal&&gs.Invaders.Counts[s].Has(Invader.Town)).ToArray();
				if(options.Length==0) return;
				var target = await spirit.Action.Choose( new TargetSpaceDecision( "Replace town with explorer", options));
				var grp = gs.Invaders.Counts[ target ];
				grp.Remove( Invader.Town );
				grp.Adjust( Invader.Explorer[1], 1 );
			}
		}

		[FearLevel( 3, "Each player may replace 1 City with 1 Town or 1 Town with 1 Explorer in a Coastal land." )]
		public async Task Level3( GameState gs ) {
			foreach(var spirit in gs.Spirits) {
				var options = gs.Island.AllSpaces.Where( s => s.IsCostal && gs.Invaders.Counts[ s ].HasAny(Invader.Town,Invader.City) ).ToArray();
				if(options.Length == 0) return;
				var target = await spirit.Action.Choose( new TargetSpaceDecision( "Replace town with explorer", options ));
				var cnts = gs.Invaders.Counts[target];
				if(cnts.Has(Invader.City)) {
					cnts.Remove( Invader.City);
					cnts.Adjust( Invader.Town[2], 1 );
				} else {
					cnts.Remove( Invader.Town );
					cnts.Adjust( Invader.Explorer[1], 1 );
				}
			}
		}

	}

}
