namespace SpiritIsland {
	public class Invader {
		static readonly public TokenCategory Explorer = new HealthTokenCategory( "Explorer", 1, true, Img.Explorer );
		static readonly public TokenCategory Town = new HealthTokenCategory( "Town", 2, true, Img.Town1, Img.Town2 );
		static readonly public TokenCategory City = new HealthTokenCategory( "City", 3, true, Img.City1, Img.City2, Img.City3 );
	}

}
