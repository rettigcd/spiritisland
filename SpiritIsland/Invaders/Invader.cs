namespace SpiritIsland {
	public class Invader {
		static readonly public TokenClass Explorer = new HealthTokenClass( "Explorer", 1, TokenCategory.Invader, Img.Explorer );
		static readonly public TokenClass Town = new HealthTokenClass( "Town", 2, TokenCategory.Invader, Img.Town1, Img.Town2 );
		static readonly public TokenClass City = new HealthTokenClass( "City", 3, TokenCategory.Invader, Img.City1, Img.City2, Img.City3 );
	}

}
