using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {
	class Unrest : IFearOptions {
		public const string Name = "Unrest";
		string IFearOptions.Name => Name;

		[FearLevel( 1, "Each player adds 1 strife to a town." )]
		public async Task Level1( FearCtx ctx ) {
			var gs = ctx.GameState;
			// Each player adds 1 strife to a town.
			foreach(var spiritCtx in ctx.Spirits)
				await spiritCtx.AddStrifeToOne(gs.Island.AllSpaces,Invader.Town);
		}

		[FearLevel( 2, "Each player adds 1 strife to a town.  For the rest of this turn, invaders have -1 health per strife to a minimum of 1." )]
		public async Task Level2( FearCtx ctx ) {

			// Each player adds 1 strife to a town.
			foreach(var spiritCtx in ctx.Spirits)
				await spiritCtx.AddStrifeToOne( ctx.GameState.Island.AllSpaces, Invader.Town );

			// For the rest of this turn, invaders have -1 health per strife to a minimum of 1.
			StrifedRavage.StrifedInvadersLoseHealthPerStrife(ctx);
		}

		[FearLevel( 3, "Each player adds 1 strife to an invader.  For the rest of this turn, invaders have -1 health per strife to a minimum of 1." )]
		public async Task Level3( FearCtx ctx ) {
			var gs = ctx.GameState;
			// Each player adds 1 strife to an invader.
			foreach(var spiritCtx in ctx.Spirits)
				await spiritCtx.AddStrifeToOne( gs.Island.AllSpaces );

			// For the rest of this turn, invaders have -1 health per strife to a minimum of 1.
			StrifedRavage.StrifedInvadersLoseHealthPerStrife(ctx);
		}

	}

}
