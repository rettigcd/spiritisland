namespace SpiritIsland;

public static class Human {

	// Invaders
	static public HumanTokenClass Explorer { get; } = new HumanTokenClass( "Explorer", TokenCategory.Invader, 0, Img.Explorer, 1 );
	static public HumanTokenClass Town { get; } = new HumanTokenClass( "Town", TokenCategory.Invader, 1, Img.Town, 2 );
	static public HumanTokenClass City { get; } = new HumanTokenClass( "City", TokenCategory.Invader, 2, Img.City, 3 );

	static public HumanTokenClass[] Explorer_Town { get; } = new HumanTokenClass[] { Explorer, Town };
	static public HumanTokenClass[] Town_City { get; } = new HumanTokenClass[] { Town, City };
	static public HumanTokenClass[] Invader { get; } = new HumanTokenClass[] { Explorer, Town, City };

	// Dahan
	static public readonly HumanTokenClass Dahan = new HumanTokenClass( "Dahan", TokenCategory.Dahan, 0, Img.Dahan, 2 );

	// Helper
	static public ITokenClass[] Plus( this IEnumerable<ITokenClass> original, params ITokenClass[] additional )
		=> original.Concat( additional ).ToArray();
}