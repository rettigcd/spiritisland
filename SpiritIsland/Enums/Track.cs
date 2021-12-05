using System.Linq;
using Elem = SpiritIsland.Element;

namespace SpiritIsland {

	public class Track : IOption {

		public static Track MkEnergy( int energy, Element el ) => new Track( energy+","+ el.ToString().ToLower() + " energy", el ) { Energy = energy };
		public static Track MkEnergy( int energy ) => new Track( energy + " energy" ) { Energy = energy };
		public static Track MkEnergy(params Element[] els ) => new Track( els.Select(x=>x.ToString()).Join(",").ToLower() + " energy", els );

		public static Track MkCard(int plays) => new Track($"{plays} cardplay" ) { CardPlay = plays };
		public static Track MkElement(params Element[] els) => new Track( els.Select(x=>x.ToString()).Join(",").ToLower(), els );

		// ! Instead of enumerating this here, we could generate them when needed in the spirit
		public static Track Energy0     => MkEnergy( 0 );
		public static Track Energy1     => MkEnergy( 1 );
		public static Track Energy2     => MkEnergy( 2 );
		public static Track Energy3     => MkEnergy( 3 );
		public static Track Energy4     => MkEnergy( 4 );
		public static Track Energy5     => MkEnergy( 5 );
		public static Track Energy6     => MkEnergy( 6 );
		public static Track Energy7     => MkEnergy( 7 );
		public static Track Energy8     => MkEnergy( 8 );
		public static Track Energy9     => MkEnergy( 9 );
		public static Track FireEnergy  => MkEnergy( Elem.Fire );
		public static Track PlantEnergy => MkEnergy( Elem.Plant );
		public static Track MoonEnergy  => MkEnergy( Elem.Moon );
		public static Track SunEnergy   => MkEnergy( Elem.Sun );
		public static Track AirEnergy   => MkEnergy( Elem.Air );
		public static Track AnyEnergy   => MkEnergy( Elem.Any );
		public static Track AnimalEnergy=> MkEnergy( Elem.Animal );
		public static Track EarthEnergy => MkEnergy( Elem.Earth );
		public static Track WaterEnergy => MkEnergy( Elem.Water );

		public static Track Card1 => MkCard(1);
		public static Track Card2 => MkCard(2);
		public static Track Card3 => MkCard(3);
		public static Track Card4 => MkCard(4);
		public static Track Card5 => MkCard(5);
		public static Track Card6 => MkCard(6);

		public static Track Push1Dahan => new Track( "Push1dahan" ){ Action = new Push1DahanFromLands() };
		public static Track Reclaim1 => new Track( "reclaim 1" ){ Action=new Reclaim1() };
		public static Track Reclaim1Energy => new Track( "reclaim 1 energy" ){ Action=new Reclaim1() };
		public static Track Energy5Reclaim1 => new Track( "5,reclaim1 energy" ){ Energy=5, Action=new Reclaim1() };
		public static Track Card5Reclaim1 => new Track( "Fivereclaimone" ){ CardPlay=5, Action=new Reclaim1() };

		public static readonly Track Destroyed = new Track("destroyed"); // only 1 of these

		public Track( string text, params Element[] els ){ this.Text = text; Elements = els; }

		public string Text {get;}

		public int? Energy { get; private set; }

		public Element[] Elements { get; }

		public int? CardPlay { get; set; }

		public IActionFactory Action { get; set; }

		public virtual void AddElement( ElementCounts elements ) {
			foreach(var el in Elements)
				elements[el]++;
		}

	}

}
