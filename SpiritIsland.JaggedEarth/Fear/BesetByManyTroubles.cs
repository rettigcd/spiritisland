namespace SpiritIsland.JaggedEarth;

public class BesetByManyTroubles : IFearOptions {
		

	public const string Name = "Beset by Many Troubles";
	string IFearOptions.Name => Name;

	[FearLevel(1, "In each land with Badlands / Beasts / Disease / Wilds / Strife, Defend 3." )]
	public Task Level1( FearCtx ctx ) {
		return Cmd.InEachLand( Cause.Fear
			, Cmd.Defend(3)
			, t=>t.Badlands.Any||t.Beasts.Any||t.Disease.Any||t.Wilds.Any||t.HasStrife // !! make HasStrife a Porperty, not an extension method
		).Execute( ctx.GameState );
	}

	[FearLevel(2, "In each land with Badlands / Beasts / Disease / Wilds / Strife, or adjacent to 3 or more such tokens, Defend 5." )]
	public Task Level2( FearCtx ctx ) {
		var counts = CountTokensOfInterest( ctx );

		foreach(var space in ctx.GameState.Island.AllSpaces)
			if( counts.ContainsKey(space) || 3 < space.Adjacent.Sum(adj=>counts[adj]) )
				ctx.GameState.Tokens[space].Defend.Add(5);

		return Task.CompletedTask;
	}

	[FearLevel(3, "Every Badlands / Beasts / Disease / Wilds / Strife grants Defend 3 in its land and adjacent lands." )]
	public Task Level3( FearCtx ctx ) {
		foreach(var space in ctx.GameState.Island.AllSpaces) {
			if(CountTokensIn(ctx,space)==0) continue;
			ctx.GameState.Tokens[space].Defend.Add(3);
			foreach(var adj in space.Adjacent)
				ctx.GameState.Tokens[adj].Defend.Add(3);
		}
		return Task.CompletedTask;
	}


	static CountDictionary<Space> CountTokensOfInterest( FearCtx ctx ) {
		return ctx.GameState.Island.AllSpaces
			.ToDictionary( s => s, s => CountTokensIn( ctx, s ) )
			.ToCountDict();
	}

	static int CountTokensIn( FearCtx ctx, Space s ) {
		var tokens = ctx.GameState.Tokens[s];
		return tokens.SumAny( TokenType.Badlands, TokenType.Beast, TokenType.Wilds ) + tokens.CountStrife();
	}

}