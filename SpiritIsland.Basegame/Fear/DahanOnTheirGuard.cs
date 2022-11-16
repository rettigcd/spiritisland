namespace SpiritIsland.Basegame;

public class DahanOnTheirGuard : IFearOptions {

	public const string Name = "Dahan on their Guard";
	string IFearOptions.Name => Name;


	[FearLevel( 1, "In each land, Defend 1 per Dahan." )]
	public Task Level1( FearCtx ctx ) {
		DefendIt( ctx.GameState, s => s.Dahan.Count );
		return Task.CompletedTask;
	}

	[FearLevel( 2, "In each land with Dahan, Defend 1, plus an additional Defend 1 per Dahan." )]
	public Task Level2( FearCtx ctx ) {
		DefendIt( ctx.GameState, s => 1 + s.Dahan.Count );
		return Task.CompletedTask;
	}

	[FearLevel( 3, "In each land, Defend 2 per Dahan." )]
	public Task Level3( FearCtx ctx ) {
		DefendIt( ctx.GameState, s => 2 * s.Dahan.Count );
		return Task.CompletedTask;
	}

	static void DefendIt( GameState gs, Func<SpaceState, int> calcDefense ) {
		foreach(var space in gs.AllActiveSpaces.Where( s=>s.Dahan.Any ))
			space.Defend.Add( calcDefense( space ) );
	}

}