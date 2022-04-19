namespace SpiritIsland;

public class Invader {
	static readonly public HealthTokenClass Explorer = new HealthTokenClass( "Explorer", 1, TokenCategory.Invader, 0 );
	static readonly public HealthTokenClass Town = new HealthTokenClass( "Town", 2, TokenCategory.Invader, 1 );
	static readonly public HealthTokenClass City = new HealthTokenClass( "City", 3, TokenCategory.Invader, 2 );
}