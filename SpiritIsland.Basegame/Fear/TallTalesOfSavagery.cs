namespace SpiritIsland.Basegame;

public class TallTalesOfSavagery : IFearOptions {

	public const string Name = "Tall Tales of Savagery";
	string IFearOptions.Name => Name;


	[FearLevel( 1, "Each player removes 1 Explorer from a land with Dahan." )]
	public async Task Level1( FearCtx ctx ) {
		// Each player
		await Cmd.EachSpirit(
			// removes 1 explorere
			Cmd.RemoveExplorers(1)
				// from a land with Dahan
				.From(x=>x.Dahan.Any && x.Tokens.Has( Invader.Explorer ),"land with Dahan")
		).Execute( ctx.GameState );
	}

	[FearLevel( 2, "Each player removes 2 Explorer or 1 Town from a land with Dahan." )]
	public async Task Level2( FearCtx ctx ) {
		// Each player
		await Cmd.EachSpirit(
			// removes 2 explorere or 1 Town
			Cmd.Pick1(Cmd.RemoveExplorers( 2 ), Cmd.RemoveTowns(1))
				// from a land with Dahan
				.From( x => x.Dahan.Any && x.Tokens.HasAny( Invader.Explorer, Invader.Town ), "land with Dahan" )
		).Execute( ctx.GameState );
	}

	[FearLevel( 3, "Remove 2 Explorer or 1 Town from each land with Dahan. Then, remove 1 City from each land with at least 2 Dahan." )]
	public async Task Level3( FearCtx ctx ) {
		var gs = ctx.GameState;
		// Remove 2 explorers or 1 Town from each land with Dahan
		foreach(var space in gs.Island.AllSpaces.Where(s => gs.DahanOn(s).Any))
			await RemoveTownOr2Explorers( gs.Invaders.On( space ) );
		// Then, remove 1 City from each land with at least 2 Dahan
		foreach(var space in gs.Island.AllSpaces.Where( s=>gs.DahanOn(s).Count>=2 && gs.Tokens[s].Has(Invader.City) ))
			await gs.Invaders.On(space).Remove(Invader.City);
	}

	static async Task RemoveTownOr2Explorers( InvaderBinding grp ) { // !! maybe we should let the player choose in case town was strifed
		if(grp.Tokens.Has(Invader.Town))
			await grp.Remove( Invader.Town );
		else
			await grp.Remove( Invader.Explorer );
			await grp.Remove( Invader.Explorer );
	}

}