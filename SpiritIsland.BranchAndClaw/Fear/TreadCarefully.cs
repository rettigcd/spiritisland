namespace SpiritIsland.BranchAndClaw;

class TreadCarefully : IFearOptions {
	public const string Name = "Tread Carefully";
	string IFearOptions.Name => Name;


	[FearLevel( 1, "Each player may choose a land with dahan or adjacent to at least 5 dahan.  Invaders do not ravage there this turn." )]
	public async Task Level1( FearCtx ctx ) {
		// Each player may choose a land with dahan or adjacent to at least 5 dahan.
		foreach(var spiritCtx in ctx.Spirits) {
			var spaceCtx = await spiritCtx.SelectSpace("Don't ravage", ctx.GameState.Island.AllSpaces.Where(ctx.WithDahanOrAdjacentTo5) );
			// Invaders do not ravage there this turn.
			ctx.GameState.SkipRavage(spaceCtx.Space);
		}
	}

	[FearLevel( 2, "Each player may choose a land with dahan or adjacent to at least 3 dahan.  Invaders do not ravage there this turn." )]
	public async Task Level2( FearCtx ctx ) {
		// Each player may choose a land with dahan or adjacent to at least 3 dahan.
		foreach(var spiritCtx in ctx.Spirits) {
			var spaceCtx = await spiritCtx.SelectSpace( "Don't ravage", ctx.GameState.Island.AllSpaces.Where( ctx.WithDahanOrAdjacentTo3 ) );
			// Invaders do not ravage there this turn.
			ctx.GameState.SkipRavage( spaceCtx.Space );
		}
	}

	[FearLevel( 3, "Each player may choose a land with dahan or adjacent to dahan.  Invaders do not ravage their this turn." )]
	public async Task Level3( FearCtx ctx ) {
		// Each player may choose a land with dahan or adjacent to dahan.
		foreach(var spiritCtx in ctx.Spirits) {
			var spaceCtx = await spiritCtx.SelectSpace( "Don't ravage", ctx.GameState.Island.AllSpaces.Where( ctx.WithDahanOrAdjacentTo1 ) );
			// Invaders do not ravage there this turn.
			ctx.GameState.SkipRavage( spaceCtx.Space );
		}
	}

}