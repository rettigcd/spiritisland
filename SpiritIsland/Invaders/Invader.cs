namespace SpiritIsland;

public class Invader {
	static readonly public HealthTokenClass Explorer = new HealthTokenClass( "Explorer", TokenCategory.Invader, 0, Img.Explorer, 1 );
	static readonly public HealthTokenClass Town = new HealthTokenClass( "Town", TokenCategory.Invader, 1, Img.Town, 2 );
	static readonly public HealthTokenClass City = new HealthTokenClass( "City", TokenCategory.Invader, 2, Img.City, 3 );
}