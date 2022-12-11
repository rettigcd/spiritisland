namespace SpiritIsland.BranchAndClaw;

public class Unrest : IFearCard {
	public const string Name = "Unrest";
	public string Text => Name;
	public int? Activation { get; set; }
	public bool Flipped { get; set; }

	[FearLevel( 1, "Each player adds 1 strife to a town." )]
	public async Task Level1( GameCtx ctx ) {
		var gs = ctx.GameState;
		// Each player adds 1 strife to a town.
		foreach(var spiritCtx in ctx.Spirits)
			await spiritCtx.AddStrifeToOne(gs.AllActiveSpaces,Invader.Town);
	}

	[FearLevel( 2, "Each player adds 1 strife to a town.  For the rest of this turn, invaders have -1 health per strife to a minimum of 1." )]
	public async Task Level2( GameCtx ctx ) {

		// Each player adds 1 strife to a town.
		foreach(var spiritCtx in ctx.Spirits)
			await spiritCtx.AddStrifeToOne( ctx.GameState.AllActiveSpaces, Invader.Town );

		// For the rest of this turn, invaders have -1 health per strife to a minimum of 1.
		await StrifedRavage.InvadersReduceHealthByStrifeCount( ctx );
	}

	[FearLevel( 3, "Each player adds 1 strife to an invader.  For the rest of this turn, invaders have -1 health per strife to a minimum of 1." )]
	public async Task Level3( GameCtx ctx ) {

		var gs = ctx.GameState;
		// Each player adds 1 strife to an invader.
		foreach(var spiritCtx in ctx.Spirits)
			await spiritCtx.AddStrifeToOne( gs.AllActiveSpaces );

		// For the rest of this turn, invaders have -1 health per strife to a minimum of 1.
		await StrifedRavage.InvadersReduceHealthByStrifeCount( ctx );
	}

}