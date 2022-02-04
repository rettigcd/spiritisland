namespace SpiritIsland.Basegame;

public class DahanOnTheirGuard : IFearOptions {

	public const string Name = "Dahan on their Guard";
	string IFearOptions.Name => Name;


	[FearLevel( 1, "In each land, Defend 1 per Dahan." )]
	public Task Level1( FearCtx ctx ) {
		var gs = ctx.GameState;
		int defend( Space space ) => gs.DahanOn( space ).Count;
		return DefendIt( gs, defend );
	}

	[FearLevel( 2, "In each land with Dahan, Defend 1, plus an additional Defend 1 per Dahan." )]
	public Task Level2( FearCtx ctx ) {
		var gs = ctx.GameState;
		int defend(Space space) => 1 + gs.DahanOn( space ).Count;
		return DefendIt( gs, defend );
	}

	[FearLevel( 3, "In each land, Defend 2 per Dahan." )]
	public Task Level3( FearCtx ctx ) {
		var gs = ctx.GameState;
		int defend( Space space ) => 2* gs.DahanOn( space ).Count;
		return DefendIt( gs, defend );
	}

	static Task DefendIt( GameState gs, Func<Space, int> d ) {
		foreach(var space in gs.Island.AllSpaces.Where( s=>gs.DahanOn(s).Any ))
			gs.Tokens[space].Defend.Add( d( space ) );
		return Task.CompletedTask;
	}

}