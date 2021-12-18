namespace SpiritIsland {
	public class Invader {
		static readonly public TokenCategory Explorer = new HealthTokenCategory( "Explorer", 1, Img.Explorer );
		static readonly public TokenCategory Town = new HealthTokenCategory( "Town", 2, Img.Town1, Img.Town2 );
		static readonly public TokenCategory City = new HealthTokenCategory( "City", 3, Img.City1, Img.City2, Img.City3 );
	}

}
