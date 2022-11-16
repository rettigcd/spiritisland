namespace SpiritIsland.BranchAndClaw;

public class Unrest : IFearOptions {
	public const string Name = "Unrest";
	string IFearOptions.Name => Name;

	[FearLevel( 1, "Each player adds 1 strife to a town." )]
	public async Task Level1( FearCtx ctx ) {
		var gs = ctx.GameState;
		// Each player adds 1 strife to a town.
		foreach(var spiritCtx in ctx.Spirits)
			await spiritCtx.AddStrifeToOne(gs.AllActiveSpaces,Invader.Town);
	}

	[FearLevel( 2, "Each player adds 1 strife to a town.  For the rest of this turn, invaders have -1 health per strife to a minimum of 1." )]
	public async Task Level2( FearCtx ctx ) {

		var actionId = Guid.NewGuid();

		// Each player adds 1 strife to a town.
		foreach(var spiritCtx in ctx.Spirits)
			await spiritCtx.AddStrifeToOne( ctx.GameState.AllActiveSpaces, Invader.Town );

		// For the rest of this turn, invaders have -1 health per strife to a minimum of 1.
		await StrifedRavage.InvadersReduceHealthByStrifeCount( ctx.GameState, actionId );
	}

	[FearLevel( 3, "Each player adds 1 strife to an invader.  For the rest of this turn, invaders have -1 health per strife to a minimum of 1." )]
	public async Task Level3( FearCtx ctx ) {

		var actionId = Guid.NewGuid();

		var gs = ctx.GameState;
		// Each player adds 1 strife to an invader.
		foreach(var spiritCtx in ctx.Spirits)
			await spiritCtx.AddStrifeToOne( gs.AllActiveSpaces );

		// For the rest of this turn, invaders have -1 health per strife to a minimum of 1.
		await StrifedRavage.InvadersReduceHealthByStrifeCount( ctx.GameState, actionId );
	}

}