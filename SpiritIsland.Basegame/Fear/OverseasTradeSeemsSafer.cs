namespace SpiritIsland.Basegame;

public class OverseasTradeSeemsSafer : IFearOptions {

	public const string Name = "Overseas Trade Seems Safer";
	string IFearOptions.Name => Name;

	[FearLevel( 1, "Defend 3 in all Coastal lands." )]
	public Task Level1( GameCtx ctx ) {
		var gs = ctx.GameState;
		DefendCostal( gs, 3 );
		return Task.CompletedTask;
	}

	[FearLevel( 2, "Defend 6 in all Coastal lands. Invaders do not Build City in Coastal lands this turn." )]
	public Task Level2( GameCtx ctx ) {
		var gs = ctx.GameState;
		DefendCostal( gs, 6 );
		SkipCostalBuild( gs, new BuildStopper( "OverseasTradeSeemsSafer(city)", Invader.City ) { Duration = BuildStopper.EDuration.AllStopsThisTurn } );
		return Task.CompletedTask;
	}

	[FearLevel( 3, "Defend 9 in all Coastal lands. Invaders do not Build in Coastal lands this turn." )]
	public Task Level3( GameCtx ctx ) {
		var gs = ctx.GameState;
		DefendCostal( gs, 9 );
		SkipCostalBuild( gs, BuildStopper.StopAll( "OverseasTradeSeemsSafer" ) );
		return Task.CompletedTask;
	}

	static void DefendCostal( GameState gs, int defense ) {
		var tm = gs.Island.Terrain_ForFear;
		var spaces = gs.AllActiveSpaces.Where( s => tm.IsCoastal( s.Space ) && tm.IsInPlay( s ) ).ToArray();
		foreach(var space in spaces )
			space.Defend.Add( defense );
	}

	static void SkipCostalBuild( GameState gs, IBuildStopper stopper ) {
		var tm = gs.Island.Terrain_ForFear;
		var spaces = gs.AllActiveSpaces.Where( s => tm.IsCoastal( s.Space ) && tm.IsInPlay( s ) ).ToArray();
		foreach(var space in spaces)
			gs.AdjustTempToken(space.Space,stopper);
	}

}