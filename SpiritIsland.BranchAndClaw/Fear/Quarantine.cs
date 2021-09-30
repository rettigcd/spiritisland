using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	public class Quarantine : IFearOptions {
		public const string Name = "Quarantine";

		[FearLevel( 1, "Explore does not affect coastal lands." )]
		public Task Level1( FearCtx ctx ) {

			// Explore does not affect costal lands.
			ExploreDoesNotAffectCoastalLands( ctx );

			return Task.CompletedTask;
		}

		[FearLevel( 2, "Explore does not affect coastal lands. Lands with disease are not a source of invaders when exploring." )]
		public Task Level2( FearCtx ctx ) {

			// Explore does not affect coastal lands.
			ExploreDoesNotAffectCoastalLands( ctx );

			// Lands with disease are not a source of invaders when exploring
			ctx.GameState.PreExplore.Add( ( laterGs, args ) => { 
				foreach(var space in ctx.LandsWithDisease())
					args.Sources.Remove(space);
				return Task.CompletedTask;
			});

			return Task.CompletedTask;
		}

		[FearLevel( 3, "Explore does not affect coastal lands.  Invaders do not act in lands with disease." )]
		public Task Level3( FearCtx ctx ) {

			// Explore does not affect coastal lands.
			ExploreDoesNotAffectCoastalLands( ctx );

			// Invaders do not act in lands with disease.
			ctx.GameState.SkipAllInvaderActions( ctx.LandsWithDisease().ToArray() );

			return Task.CompletedTask;
		}

		static void ExploreDoesNotAffectCoastalLands( FearCtx ctx ) {
			var gs = ctx.GameState;
			gs.SkipExplore( gs.Island.AllSpaces.Where( x => x.IsCostal ).ToArray() );
		}
	}

}
