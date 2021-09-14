using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class AvoidTheDahan : IFearOptions {
		public const string Name = "Avoid the Dahan";

		[FearLevel(1, "Invaders do not Explore into lands with at least 2 Dahan." )]
		public Task Level1( FearCtx ctx ) {
			var gs = ctx.GameState;
			gs.SkipExplore( gs.Island.AllSpaces.Where( s => gs.DahanGetCount( s ) >= 2 ).ToArray() );
			return Task.CompletedTask;
		}

		[FearLevel( 2, "Invaders do not Build in lands where Dahan outnumber Town / City." )]
		public Task Level2( FearCtx ctx ) {
			var gs = ctx.GameState;
			gs.SkipBuild( gs.Island.AllSpaces
				.Where( s => gs.Tokens[s].TownsAndCitiesCount() < gs.DahanGetCount( s)
				).ToArray()
			);
			return Task.CompletedTask;
		}

		[FearLevel( 3, "Invaders do not Build in lands with Dahan." )]
		public Task Level3( FearCtx ctx ) {
			var gs = ctx.GameState;
			gs.SkipBuild( gs.Island.AllSpaces
				.Where( s => 0 < gs.DahanGetCount( s ) ).ToArray()
			);
			return Task.CompletedTask;
		}

	}

}

