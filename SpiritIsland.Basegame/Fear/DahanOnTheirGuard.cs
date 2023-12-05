namespace SpiritIsland.Basegame;

public class DahanOnTheirGuard : FearCardBase, IFearCard {

	public const string Name = "Dahan on their Guard";
	public string Text => Name;

	[FearLevel( 1, "In each land, Defend 1 per Dahan." )]
	public Task Level1( GameState ctx ) {
		DefendIt( ctx, s => s.Dahan.CountAll );
		return Task.CompletedTask;
	}

	[FearLevel( 2, "In each land with Dahan, Defend 1, plus an additional Defend 1 per Dahan." )]
	public Task Level2( GameState ctx ) {
		DefendIt( ctx, s => 1 + s.Dahan.CountAll );
		return Task.CompletedTask;
	}

	[FearLevel( 3, "In each land, Defend 2 per Dahan." )]
	public Task Level3( GameState ctx ) {
		DefendIt( ctx, s => 2 * s.Dahan.CountAll );
		return Task.CompletedTask;
	}

	static void DefendIt( GameState gs, Func<SpaceState, int> calcDefense ) {
		foreach(var space in gs.Spaces.Where( s=>s.Dahan.Any ))
			space.Defend.Add( calcDefense( space ) );
	}

}