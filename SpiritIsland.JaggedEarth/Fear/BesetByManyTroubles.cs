namespace SpiritIsland.JaggedEarth;

public class BesetByManyTroubles : IFearOptions {
		

	public const string Name = "Beset by Many Troubles";
	string IFearOptions.Name => Name;

	[FearLevel(1, "In each land with Badlands / Beasts / Disease / Wilds / Strife, Defend 3." )]
	public Task Level1( FearCtx ctx ) {
		return Cmd.InEachLand( 
			Cmd.Defend(3)
			, t=>t.Badlands.Any||t.Beasts.Any||t.Disease.Any||t.Wilds.Any||t.HasStrife // !! make HasStrife a Porperty, not an extension method
		).Execute( ctx.GameState );
	}

	[FearLevel(2, "In each land with Badlands / Beasts / Disease / Wilds / Strife, or adjacent to 3 or more such tokens, Defend 5." )]
	public Task Level2( FearCtx ctx ) {
		var counts = CountTokensOfInterest( ctx );

		foreach(var space in ctx.GameState.AllActiveSpaces)
			if( counts.ContainsKey(space.Space) || 3 <= space.Adjacent.Sum(adj=>counts[adj.Space]) )
				space.Defend.Add(5);

		return Task.CompletedTask;
	}

	[FearLevel(3, "Every Badlands / Beasts / Disease / Wilds / Strife grants Defend 3 in its land and adjacent lands." )]
	public Task Level3( FearCtx ctx ) {
		foreach(var space in ctx.GameState.AllActiveSpaces) {
			if( CountTokensIn(space)==0 ) continue;
			space.Defend.Add(3);
			foreach(var adj in space.Adjacent)
				adj.Defend.Add(3);
		}
		return Task.CompletedTask;
	}


	static CountDictionary<Space> CountTokensOfInterest( FearCtx ctx ) {
		return ctx.GameState.AllActiveSpaces
			.ToDictionary( s => s.Space, s => CountTokensIn( s ) )
			.ToCountDict();
	}

	static int CountTokensIn( SpaceState ss )
		=> ss.SumAny( TokenType.Badlands, TokenType.Beast, TokenType.Wilds ) + ss.CountStrife();

}