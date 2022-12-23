namespace SpiritIsland.Basegame;

public class OverseasTradeSeemsSafer : IFearCard {

	public const string Name = "Overseas Trade Seems Safer";
	public string Text => Name;
	public int? Activation { get; set; }
	public bool Flipped { get; set; }

	[FearLevel( 1, "Defend 3 in all Coastal lands." )]
	public Task Level1( GameCtx ctx ) {
		foreach(var space in GetCoastalSpaces( ctx.GameState ))
			// Defend 3 in all Coastal lands.
			space.Defend.Add( 3 );

		return Task.CompletedTask;
	}

	[FearLevel( 2, "Defend 6 in all Coastal lands. Invaders do not Build City in Coastal lands this turn." )]
	public Task Level2( GameCtx ctx ) {
		var gs = ctx.GameState;
		foreach( var space in GetCoastalSpaces( gs )) {
			// Defend 6 in all Coastal lands.
			space.Defend.Add( 6 );
			// Invaders do not Build City in Coastal lands this turn.
			ctx.GameState.SkipAllBuilds( space.Space, $"{Name}(city)", Invader.City );
		}

		return Task.CompletedTask;
	}

	[FearLevel( 3, "Defend 9 in all Coastal lands. Invaders do not Build in Coastal lands this turn." )]
	public Task Level3( GameCtx ctx ) {

		foreach( var space in GetCoastalSpaces( ctx.GameState )) {
			// Defend 9 in all Coastal lands.
			space.Defend.Add( 9 );
			// Invaders do not Build in Coastal lands this turn.
			ctx.GameState.SkipAllBuilds( space.Space, Name );
		}

		return Task.CompletedTask;
	}

	static SpaceState[] GetCoastalSpaces( GameState gs ) {
		var tm = gs.Island.Terrain_ForFear;
		var spaces = gs.AllActiveSpaces.Where( s => tm.IsCoastal( s ) && tm.IsInPlay( s ) ).ToArray();
		return spaces;
	}
}