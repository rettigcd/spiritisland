﻿namespace SpiritIsland.BranchAndClaw;

class TreadCarefully : IFearOptions {

	public const string Name = "Tread Carefully";
	string IFearOptions.Name => Name;

	[FearLevel( 1, "Each player may choose a land with dahan or adjacent to at least 5 dahan.  Invaders do not ravage there this turn." )]
	public Task Level1( FearCtx ctx ) {
		return StopRagageInDahanSpaceOrAdjacentTo( ctx, 5 );
	}

	[FearLevel( 2, "Each player may choose a land with dahan or adjacent to at least 3 dahan.  Invaders do not ravage there this turn." )]
	public Task Level2( FearCtx ctx ) {
		return StopRagageInDahanSpaceOrAdjacentTo( ctx, 3 );
	}

	[FearLevel( 3, "Each player may choose a land with dahan or adjacent to dahan.  Invaders do not ravage their this turn." )]
	public Task Level3( FearCtx ctx ) {
		return StopRagageInDahanSpaceOrAdjacentTo( ctx, 1 );
	}

	Task StopRagageInDahanSpaceOrAdjacentTo( FearCtx ctx, int adjacentDahanThreshold ) {
		// Each player
		return Cmd.EachSpirit(
			// Invaders do not ravage there this turn.
			stopRavage
				// MAY choose a land with dahan or adjacent to at least # dahan.		// !!! MAY - make it optional
				// !! It would be handy to wrap these 2 parameters into a single VerboseSpaceFilter type/class so that (a) reverse the .In() order, and Reuse easier.
				.In( ctx => ctx.Dahan.Any || ctx.AdjacentCtxs.Sum( x => x.Dahan.Count ) >= adjacentDahanThreshold, $"a land with dahan or adjacent to at least {adjacentDahanThreshold} dahan" )
		).Execute( ctx.GameState );
	}

	readonly SpaceAction stopRavage = new SpaceAction( "Invaders do not ravage there this turn.", ctx => { 
		// Skip twice in case there are 2 ravage cards.
		ctx.GameState.SkipRavage( ctx.Space );
		ctx.GameState.SkipRavage( ctx.Space );
	});

}