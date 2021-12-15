using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	class PanickedByWildBeasts : IFearOptions {
		public const string Name = "Panicked by Wild Beasts";

		[FearLevel( 1, "Each player adds 1 strife in a land with or adjacent to beast" )]
		public async Task Level1( FearCtx ctx ) {

			// Each player adds 1 strife in a land with or adjacent to beast
			foreach(var spiritCtx in ctx.Spirits)
				await spiritCtx.AddStrifeToOne(ctx.LandsWithOrAdjacentToBeasts());
		}

		[FearLevel( 2, "Each player adds 1 strife in a land with or adjacent to beast.  Invaders skip their normal explore and build in lands ith beast" )]
		public async Task Level2( FearCtx ctx ) {

			// Each player adds 1 strife in a land with or adjacent to beast.
			foreach(var spiritCtx in ctx.Spirits)
				await spiritCtx.AddStrifeToOne( ctx.LandsWithOrAdjacentToBeasts() );

			// Invaders skip their normal explore and build in lands ith beast
			foreach(var land in ctx.GameState.Island.AllSpaces)
				if(ctx.GameState.Tokens[land].Beasts.Any) {
					ctx.GameState.SkipExplore( land );
					ctx.GameState.Skip1Build( land );
				}
		}

		[FearLevel( 3, "Each player adds 1 strife in a land with or adjacent to beast.  Invaders skip all normal actions in lands with beast." )]
		public async Task Level3( FearCtx ctx ) {

			// Each player adds 1 strife in a land with or adjacent to beast.
			foreach(var spiritCtx in ctx.Spirits)
				await spiritCtx.AddStrifeToOne( ctx.LandsWithOrAdjacentToBeasts() );

			// Invaders skip all normal actions in lands with beast.
			foreach(var land in ctx.GameState.Island.AllSpaces)
				if(ctx.GameState.Tokens[land].Beasts.Any)
					ctx.GameState.SkipAllInvaderActions( land );
		}

	}

}
