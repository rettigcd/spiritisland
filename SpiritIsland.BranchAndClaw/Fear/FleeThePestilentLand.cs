namespace SpiritIsland.BranchAndClaw;

class FleeThePestilentLand : IFearOptions {

	public const string Name = "Flee the Pestilent Land";
	string IFearOptions.Name => Name;

	[FearLevel( 1, "Each player removes 1 explorer/town from a land with disease" )]
	public Task Level1( FearCtx ctx ) {

		// each player 
		return Cmd.EachSpirit(
			// removes 1 explorer/town 
			Cmd.RemoveExplorersOrTowns(1)
				// from a land with disease
				.From( spaceCtx => spaceCtx.Tokens.Disease.Any && spaceCtx.Tokens.HasInvaders(), "land with disease" )
		).Execute( ctx.GameState );

	}

	[FearLevel( 2, "Each player removes up to 3 health of invaders from a land with disease or 1 explorer from an inland land" )]
	public Task Level2( FearCtx ctx ) {
		// each player 
		return Cmd.EachSpirit( 
			Cmd.Pick1<SelfCtx>(
				// removes up to 3 health of invaders from a land with disease
				RemoveNHealthOfInvadersFromDisease(3,ctx.GameState),
				// or 1 explorer from an inland land
				Cmd.RemoveExplorers(1).From( ctx => ctx.IsInland, "an inland land" )
			)
		).Execute(ctx.GameState);
	}

	[FearLevel( 3, "each player removes up to 5 health of invaders from a land with disease or 1 explorer/town from an inland land" )]
	public Task Level3( FearCtx ctx ) {
		// each player 
		return Cmd.EachSpirit( 
			Cmd.Pick1<SelfCtx>(
				// removes up to 5 health of invaders from a land with disease
				RemoveNHealthOfInvadersFromDisease(5,ctx.GameState),
				// or 1 explorer / Town from an inland land
				Cmd.RemoveExplorersOrTowns(1).From( s=>s.IsInland, "an inland land" )
			)
		).Execute(ctx.GameState);
	}

	static DecisionOption<SelfCtx> RemoveNHealthOfInvadersFromDisease( int healthToRemove, GameState _ ) =>
		Cmd.RemoveUpToNHealthOfInvaders(healthToRemove).From( ctx => ctx.Tokens.Disease.Any && ctx.Tokens.HasInvaders(), "land with disease" );

}