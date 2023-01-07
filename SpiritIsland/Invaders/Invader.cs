namespace SpiritIsland;

public static class Invader {
	static public HealthTokenClass Explorer { get; } = new HealthTokenClass( "Explorer", TokenCategory.Invader, 0, Img.Explorer, 1 );
	static public HealthTokenClass Town { get; } = new HealthTokenClass( "Town", TokenCategory.Invader, 1, Img.Town, 2 );
	static public HealthTokenClass City { get; } = new HealthTokenClass( "City", TokenCategory.Invader, 2, Img.City, 3 );

	static public HealthTokenClass[] Explorer_Town { get; } = new HealthTokenClass[] { Explorer, Town };
	static public HealthTokenClass[] Town_City { get; } = new HealthTokenClass[] { Town, City };
	static public HealthTokenClass[] Any { get; } = new HealthTokenClass[] { Explorer, Town, City };

	static public TokenClass[] Plus( this IEnumerable<TokenClass> original, params TokenClass[] additional) 
		=> original.Concat(additional).ToArray();
}