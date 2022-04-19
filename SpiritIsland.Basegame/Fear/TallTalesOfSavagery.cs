namespace SpiritIsland.Basegame;

public class TallTalesOfSavagery : IFearOptions {

	public const string Name = "Tall Tales of Savagery";
	string IFearOptions.Name => Name;


	[FearLevel( 1, "Each player removes 1 Explorer from a land with Dahan." )]
	public async Task Level1( FearCtx ctx ) {
		var gs = ctx.GameState;
		foreach(var spirit in gs.Spirits) {
			var options = gs.Island.AllSpaces.Where( s => gs.DahanOn( s ).Any && gs.Tokens[s].Has( Invader.Explorer ) ).ToArray();
			if(options.Length == 0) return;
			await RemoveExplorerFromOneOfThese( gs, spirit, options );
		}
	}

	static async Task RemoveExplorerFromOneOfThese( GameState gs, Spirit spirit, Space[] options ) {
		var target = await spirit.Action.Decision( new Select.Space( "Select land to remove explorer", options, Present.Always ) );
		gs.Tokens[target].AdjustDefault( Invader.Explorer, -1 );
	}

	[FearLevel( 2, "Each player removes 2 Explorer or 1 Town from a land with Dahan." )]
	public async Task Level2( FearCtx ctx ) {
		var gs = ctx.GameState;
		foreach(var spirit in gs.Spirits) {
			var options = gs.Island.AllSpaces.Where( s => gs.DahanOn( s ).Any && gs.Tokens[ s ].Has(Invader.Explorer) ).ToArray();
			if(options.Length == 0) return;
			var space = await spirit.Action.Decision( new Select.Space( "Fear:select land with dahan to remove explorer", options, Present.Always ));
			await RemoveTownOr2Explorers( gs.Invaders.On( space ) );
		}
	}

	[FearLevel( 3, "Remove 2 Explorer or 1 Town from each land with Dahan. Then, remove 1 City from each land with at least 2 Dahan." )]
	public async Task Level3( FearCtx ctx ) {
		var gs = ctx.GameState;
		foreach(var space in gs.Island.AllSpaces.Where(s => gs.DahanOn(s).Any))
			await RemoveTownOr2Explorers( gs.Invaders.On( space ) );
		foreach(var space in gs.Island.AllSpaces.Where( s=>gs.DahanOn(s).Count>=2 && gs.Tokens[s].Has(Invader.City) ))
			await gs.Invaders.On(space).Remove(Invader.City);
	}

	static async Task RemoveTownOr2Explorers( InvaderBinding grp ) {
		if(grp.Tokens.Has(Invader.Town))
			await grp.Remove( Invader.Town );
		else
			await grp.Remove( Invader.Explorer );
			await grp.Remove( Invader.Explorer );
	}

}