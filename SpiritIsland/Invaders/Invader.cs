namespace SpiritIsland {
	public class Invader {
		static readonly public TokenGroup Explorer = new HealthTokenGroup( "Explorer", 1, Img.Explorer );
		static readonly public TokenGroup Town = new HealthTokenGroup( "Town", 2, Img.Town1, Img.Town2 );
		static readonly public TokenGroup City = new HealthTokenGroup( "City", 3, Img.City1, Img.City2, Img.City3 );
	}

}
