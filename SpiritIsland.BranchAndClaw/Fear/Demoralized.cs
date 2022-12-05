namespace SpiritIsland.BranchAndClaw;

public class Demoralized : IFearOptions {

	public const string Name = "Demoralized";
	string IFearOptions.Name => Name;

	[FearLevel( 1, "Defend 1 in all lands" )]
	public Task Level1( GameCtx ctx ) {
		// Defend 1 in all lands
		DefendLands( ctx, 1 );
		return Task.CompletedTask;
	}

	[FearLevel( 2, "Defend 2 in all lands" )]
	public Task Level2( GameCtx ctx ) {
		// Defend 2 in all lands
		DefendLands( ctx, 2 );
		return Task.CompletedTask;
	}

	[FearLevel( 3, "Defend 3 in all lands" )]
	public Task Level3( GameCtx ctx ) {
		// Defend 3 in all lands
		DefendLands( ctx, 3 );
		return Task.CompletedTask;
	}

	static void DefendLands( GameCtx ctx, int defense ) {
		var terrainMapper = ctx.GameState.Island.Terrain_ForFear;
		var lands = ctx.GameState.AllActiveSpaces
			.Where( x=> terrainMapper.IsInPlay(x.Space) );
		foreach(var land in lands)
			land.Defend.Add( defense );
	}

}