using Elem = SpiritIsland.Element;

namespace SpiritIsland {

	public class Track : IOption {
		static Track MkEnergy( int energy ) { return new Track( energy + " energy" ) { Energy = energy }; }
		static Track MkEnergy( Element el ) { return new Track( el.ToString().ToLower() + " energy" ) { Element = el }; }
		// ! Instead of enumerating this here, we could generate them when needed in the spirit
		public static readonly Track Energy0     = MkEnergy( 0 );
		public static readonly Track Energy1     = MkEnergy( 1 );
		public static readonly Track Energy2     = MkEnergy( 2 );
		public static readonly Track Energy3     = MkEnergy( 3 );
		public static readonly Track Energy4     = MkEnergy( 4 );
		public static readonly Track Energy5     = MkEnergy( 5 );
		public static readonly Track Energy6     = MkEnergy( 6 );
		public static readonly Track Energy7     = MkEnergy( 7 );
		public static readonly Track Energy8     = MkEnergy( 8 );
		public static readonly Track Energy9     = MkEnergy( 9 );
		public static readonly Track FireEnergy  = MkEnergy( Elem.Fire );
		public static readonly Track PlantEnergy = MkEnergy( Elem.Plant );
		public static readonly Track MoonEnergy  = MkEnergy( Elem.Moon );
		public static readonly Track SunEnergy   = MkEnergy( Elem.Sun );
		public static readonly Track AirEnergy   = MkEnergy( Elem.Air );
		public static readonly Track AnyEnergy   = MkEnergy( Elem.Any );
		public static readonly Track AnimalEnergy = MkEnergy( Elem.Animal );
		public static readonly Track EarthEnergy = MkEnergy( Elem.Earth );
		public static readonly Track WaterEnergy = MkEnergy( Elem.Water );

		public static Track MkCard(int plays) => new Track($"{plays} cardplay" ) { CardPlay = plays };
		public static readonly Track Card1 = MkCard(1);
		public static readonly Track Card2 = MkCard(2);
		public static readonly Track Card3 = MkCard(3);
		public static readonly Track Card4 = MkCard(4);
		public static readonly Track Card5 = MkCard(5);
		public static readonly Track Card6 = MkCard(6);

		public static readonly Track Reclaim1 = new Track( "reclaim 1" ){ ReclaimOne=true };

		Track( string text){ this.Text = text; }

		public string Text {get; }
		public int? Energy { get; private set; }
		public Element? Element { get; private set; }
		public int? CardPlay { get; set; }
		public bool ReclaimOne { get; set; }
	}


}
