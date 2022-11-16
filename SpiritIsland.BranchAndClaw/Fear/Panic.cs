namespace SpiritIsland.BranchAndClaw;

public class Panic : IFearOptions {
	public const string Name = "Panic";
	string IFearOptions.Name => Name;


	[FearLevel( 1, "Each player adds 1 strife in a land with beast/disease/dahan." )]
	public async Task Level1( FearCtx ctx ) {

		// Each player adds 1 strife in a land with beast/disease/dahan.
		foreach(SelfCtx spirit in ctx.Spirits)
			await spirit.AddStrifeToOne( ctx.LandsWithBeastDiseaseDahan() );

	}

	[FearLevel( 2, "Each player adds 1 strife in a land with beast/disease/dahan.  For the rest of this turn, invaders have -1 health per strife to a minimum of 1" )]
	public async Task Level2( FearCtx ctx ) {

		Guid actionId = Guid.NewGuid();

		// Each player adds 1 strife in a land with beast/disease/dahan.
		foreach(SelfCtx spirit in ctx.Spirits)
			await spirit.AddStrifeToOne( ctx.LandsWithBeastDiseaseDahan() );

		// For the rest of this turn, invaders have -1 health per strife to a minimum of 1
		await StrifedRavage.InvadersReduceHealthByStrifeCount( ctx.GameState, actionId );
	}

	[FearLevel( 3, "Each player adds 1 strife to an invader.  For the rest of this turn, invaders have -1 health per strife to a minimum of 1." )]
	public async Task Level3( FearCtx ctx ) {

		var actionId = Guid.NewGuid();

		// Each player adds 1 strife to an invader.
		foreach(SelfCtx spirit in ctx.Spirits)
			await spirit.AddStrifeToOne( ctx.GameState.AllActiveSpaces );

		// For the rest of this turn, invaders have -1 health per strife to a minimum of 1.
		await StrifedRavage.InvadersReduceHealthByStrifeCount( ctx.GameState, actionId );
	}

}