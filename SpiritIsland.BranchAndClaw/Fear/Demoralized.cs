using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	public class Demoralized : IFearOptions {

		public const string Name = "Demoralized";

		[FearLevel( 1, "Defend 1 in all lands" )]
		public Task Level1( FearCtx ctx ) {
			// Defend 1 in all lands
			DefendAllLands( ctx, 1 );
			return Task.CompletedTask;
		}

		[FearLevel( 2, "Defend 2 in all lands" )]
		public Task Level2( FearCtx ctx ) {
			// Defend 2 in all lands
			DefendAllLands( ctx, 2 );
			return Task.CompletedTask;
		}

		[FearLevel( 3, "Defend 3 in all lands" )]
		public Task Level3( FearCtx ctx ) {
			// Defend 3 in all lands
			DefendAllLands( ctx, 3 );
			return Task.CompletedTask;
		}

		static void DefendAllLands( FearCtx ctx, int defense ) {
			foreach(var space in ctx.GameState.Island.AllSpaces)
				ctx.GameState.Tokens[space].Defend.Count += defense;
		}

	}

}
