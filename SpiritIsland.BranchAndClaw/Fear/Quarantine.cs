namespace SpiritIsland.BranchAndClaw;

public class Quarantine : IFearCard {
	public const string Name = "Quarantine";
	public string Text => Name;
	public int? Activation { get; set; }
	public bool Flipped { get; set; }

	[FearLevel( 1, "Explore does not affect coastal lands." )]
	public Task Level1( GameCtx ctx ) {

		// Explore does not affect costal lands.
		ExploreDoesNotAffectCoastalLands( ctx );

		return Task.CompletedTask;
	}

	[FearLevel( 2, "Explore does not affect coastal lands. Lands with disease are not a source of invaders when exploring." )]
	public Task Level2( GameCtx ctx ) {

		// Explore does not affect coastal lands.
		ExploreDoesNotAffectCoastalLands( ctx );

		// Lands with disease are not a source of invaders when exploring
		ctx.GameState.AdjustTempTokenForAll( new SkipExploreFrom( Name ) );

		return Task.CompletedTask;
	}

	[FearLevel( 3, "Explore does not affect coastal lands.  Invaders do not act in lands with disease." )]
	public Task Level3( GameCtx ctx ) {

		// Explore does not affect coastal lands.
		ExploreDoesNotAffectCoastalLands( ctx );

		// Invaders do not act in lands with disease.
		foreach(var target in ctx.LandsWithDisease())
			ctx.GameState.SkipAllInvaderActions( target.Space, Name );

		return Task.CompletedTask;
	}

	static void ExploreDoesNotAffectCoastalLands( GameCtx ctx ) {
		var gs = ctx.GameState;
		foreach(var costal in gs.AllActiveSpaces.Where( x => x.Space.IsCoastal ))
			gs.Skip1Explore( costal, Name );
	}

}