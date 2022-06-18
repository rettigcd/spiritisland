namespace SpiritIsland.Basegame;

public class OverseasTradeSeemSafer : IFearOptions {

	public const string Name = "Overseas Trade Seem Safer";
	string IFearOptions.Name => Name;

	[FearLevel( 1, "Defend 3 in all Coastal lands." )]
	public Task Level1( FearCtx ctx ) {
		var gs = ctx.GameState;
		DefendCostal( gs, 3 );
		return Task.CompletedTask;
	}

	[FearLevel( 2, "Defend 6 in all Coastal lands. Invaders do not Build City in Coastal lands this turn." )]
	public Task Level2( FearCtx ctx ) {
		var gs = ctx.GameState;
		DefendCostal( gs, 6 );
		SkipCostalBuild( gs, new BuildStopper("OverseasTradeStopsCoastalCity", 'o', Img.None, Invader.City ));
		return Task.CompletedTask;
	}

	[FearLevel( 3, "Defend 9 in all Coastal lands. Invaders do not Build in Coastal lands this turn." )]
	public Task Level3( FearCtx ctx ) {
		var gs = ctx.GameState;
		DefendCostal( gs, 9 );
		SkipCostalBuild( gs, new BuildStopper( "OverseasTradeStopsCoastalBuilds", 'o', Img.None, Invader.City, Invader.Town ));
		return Task.CompletedTask;
	}

	static void DefendCostal( GameState gs, int defense ) {
		foreach(var space in gs.Island.AllSpaces.Where( s => s.IsCoastal ))
			gs.Tokens[space].Defend.Add( defense );
	}

	static void SkipCostalBuild( GameState gs, IBuildStopper stopper ) {
		var spaces = gs.Island.AllSpaces.Where( s => s.IsCoastal ).ToArray();
		foreach(var space in spaces)
			gs.Skip1Build(space,stopper); // !!! This should stop multiple builds, not just 1.. (Pour time sideways, multiple build cards, etc.)
	}

}