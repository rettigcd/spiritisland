namespace SpiritIsland {
	public class Invader {
		static readonly public HealthTokenClass Explorer = new HealthTokenClass( "Explorer", 1, TokenCategory.Invader, Img.Explorer );
		static readonly public HealthTokenClass Town = new HealthTokenClass( "Town", 2, TokenCategory.Invader, Img.Town1, Img.Town2 );
		static readonly public HealthTokenClass City = new HealthTokenClass( "City", 3, TokenCategory.Invader, Img.City1, Img.City2, Img.City3 );
	}

}
