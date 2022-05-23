namespace SpiritIsland;

public static class TokenCountDictionary_Extensions {

	#region Generic - Single

	static public Token[] OfType( this TokenCountDictionary counts, TokenClass tokenClass )
		=> counts.Keys.Where( x => x.Class == tokenClass ).ToArray();

	static public bool Has( this TokenCountDictionary counts, TokenClass inv )
		=> counts.OfType( inv ).Any();

	static public int Sum( this TokenCountDictionary counts, TokenClass tokenClass )
		=> counts.OfType( tokenClass ).Sum( k => counts[k] );

	#endregion

	#region Generic - Multiple (Any)

	static public Token[] OfAnyType( this TokenCountDictionary counts, params TokenClass[] healthyTypes )
		=> counts.Keys.Where( specific => healthyTypes.Contains( specific.Class ) ).ToArray();

	static public HealthToken[] OfAnyType( this TokenCountDictionary counts, params HealthTokenClass[] healthyTypes )
		=> counts.Keys.Where( specific => healthyTypes.Contains( specific.Class ) ).Cast<HealthToken>().ToArray();

	static public bool HasAny( this TokenCountDictionary counts, params TokenClass[] healthyInvaders )
		=> counts.OfAnyType( healthyInvaders ).Any();

	static public int SumAny( this TokenCountDictionary counts, params TokenClass[] healthyInvaders )
		=> counts.OfAnyType( healthyInvaders ).Sum( k => counts[k] );


	#endregion

}