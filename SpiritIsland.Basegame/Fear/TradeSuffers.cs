using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class TradeSuffers : IFearCard {

		public const string Name = "Trade Suffers";

		[FearLevel( 1, "Invaders do not Build in lands with City." )]
		public Task Level1( GameState gs ) {
			gs.SkipBuild( gs.Island.AllSpaces.Where( s => gs.Tokens[ s ].Has(Invader.City) ).ToArray() );
			return Task.CompletedTask;
		}

		[FearLevel( 2, "Each player may replace 1 Town with 1 Explorer in a Coastal land." )]
		public async Task Level2( GameState gs ) {
			foreach(var spirit in gs.Spirits) {
				var options = gs.Island.AllSpaces.Where( s => s.IsCostal && gs.Tokens[s].Has( Invader.Town ) ).ToArray();
				if(options.Length == 0) return;
				var target = await spirit.Action.Decision( new Decision.TargetSpace( "Replace town with explorer", options ) );
				await Replace.Downgrade( spirit, gs.Invaders.On( target, Cause.Fear ), Invader.Town );
			}
		}

		[FearLevel( 3, "Each player may replace 1 City with 1 Town or 1 Town with 1 Explorer in a Coastal land." )]
		public async Task Level3( GameState gs ) {
			foreach(var spirit in gs.Spirits) {
				var options = gs.Island.AllSpaces.Where( s => s.IsCostal && gs.Tokens[ s ].HasAny(Invader.Town,Invader.City) ).ToArray();
				if(options.Length == 0) return;
				var target = await spirit.Action.Decision( new Decision.TargetSpace( "Replace town with explorer or city with town", options ));
				await Replace.Downgrade( spirit, gs.Invaders.On( target, Cause.Fear ), Invader.City, Invader.Town );
			}
		}

	}

}
