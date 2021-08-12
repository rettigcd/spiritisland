namespace SpiritIsland {

	public class Track : IOption { 
		public static readonly Track Energy1 = new Track( "1 energy" );
		public static readonly Track Energy2 = new Track( "2 energy" );
		public static readonly Track Energy3 = new Track( "3 energy" );
		public static readonly Track Energy4 = new Track( "4 energy" );
		public static readonly Track Energy5 = new Track( "5 energy" );
		public static readonly Track Energy6 = new Track( "6 energy" );
		public static readonly Track Energy7 = new Track( "7 energy" );
		public static readonly Track Energy8 = new Track( "8 energy" );

		public static readonly Track FireEnergy = new Track( "fire energy" );
		public static readonly Track PlantEnergy = new Track( "plant energy" );
		public static readonly Track MoonEnergy = new Track( "moon energy" );
		public static readonly Track SunEnergy = new Track( "sun energy" );
		public static readonly Track AirEnergy = new Track( "air energy" );
		public static readonly Track AnyEnergy = new Track( "any energy" );
		public static readonly Track EarthSunEnergy = new Track( "earth energy" );

		public static readonly Track Card1 = new Track( "1 cardPlay" );
		public static readonly Track Card2 = new Track( "2 cardPlay" );
		public static readonly Track Card3 = new Track( "3 cardPlay" );
		public static readonly Track Card4 = new Track( "4 cardPlay" );
		public static readonly Track Card5 = new Track( "5 cardPlay" );
		public static readonly Track Card6 = new Track( "6 cardPlay" );

		protected Track(string text){ this.Text = text; }

		public string Text {get;}
	}


}
