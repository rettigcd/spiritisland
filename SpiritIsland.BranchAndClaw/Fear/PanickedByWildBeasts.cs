namespace SpiritIsland.BranchAndClaw;

public class PanickedByWildBeasts : IFearCard {
	public const string Name = "Panicked by Wild Beasts";
	public string Text => Name;
	public int? Activation { get; set; }
	public bool Flipped { get; set; }


	[FearLevel( 1, "Each player adds 1 strife in a land with or adjacent to beast" )]
	public async Task Level1( GameCtx ctx ) {

		// Each player adds 1 strife in a land with or adjacent to beast
		foreach(var spiritCtx in ctx.Spirits)
			await spiritCtx.AddStrifeToOne(ctx.LandsWithOrAdjacentToBeasts());
	}

	[FearLevel( 2, "Each player adds 1 strife in a land with or adjacent to beast.  Invaders skip their normal explore and build in lands ith beast" )]
	public async Task Level2( GameCtx ctx ) {

		// Each player adds 1 strife in a land with or adjacent to beast.
		foreach(var spiritCtx in ctx.Spirits)
			await spiritCtx.AddStrifeToOne( ctx.LandsWithOrAdjacentToBeasts() );

		// Invaders skip their normal explore and build in lands with beast
		foreach(var land in ctx.GameState.AllActiveSpaces)
			if(land.Beasts.Any) {
				ctx.GameState.Skip1Explore( land, Name );
				ctx.GameState.Skip1Build( land.Space, Name );
			}
	}

	[FearLevel( 3, "Each player adds 1 strife in a land with or adjacent to beast.  Invaders skip all normal actions in lands with beast." )]
	public async Task Level3( GameCtx ctx ) {

		// Each player adds 1 strife in a land with or adjacent to beast.
		foreach(var spiritCtx in ctx.Spirits)
			await spiritCtx.AddStrifeToOne( ctx.LandsWithOrAdjacentToBeasts() );

		// Invaders skip all normal actions in lands with beast.
		foreach(var land in ctx.GameState.AllActiveSpaces)
			if(land.Beasts.Any)
				ctx.GameState.SkipAllInvaderActions( land.Space, Name );
	}

}