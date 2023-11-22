namespace SpiritIsland;


public class Track : IOption {

	public static Track MkEnergy( int energy, Element el ) => new Track( energy+","+ el.ToString().ToLower() + " energy", el ) { 
		Energy = energy,
		Icon = new IconDescriptor { 
			BackgroundImg = Img.Coin, Text = energy.ToString(), 
			Sub = new IconDescriptor{ BackgroundImg = el.GetTokenImg() } 
		}
	};

	public static Track MkEnergy( int energy ) => new Track( energy + " energy" ) { 
		Energy = energy, 
		Icon = new IconDescriptor{ BackgroundImg = Img.Coin, Text = energy.ToString() },
	};

	public static Track MkEnergy( params Element[] els ) {
		var track = new Track( els.Select( x => x.ToString() ).Join( "," ).ToLower() + " energy", els ) {
			Icon = new IconDescriptor { BackgroundImg = Img.Coin, ContentImg = els[0].GetTokenImg() }
		};
		if(els.Length==2)
			track.Icon.ContentImg2 = els[1].GetTokenImg();
		return track;
	}

	public static Track MkCard(int plays) => new Track($"{plays} cardplay" ) { 
		CardPlay = plays,
		Icon = new IconDescriptor { BackgroundImg = Img.CardPlay, Text = plays.ToString() }
	};

	public static Track MkCard( Element el ) => new Track( el.ToString().ToLower(), el ) {
		Icon = new IconDescriptor { ContentImg = el.GetTokenImg() }
	};

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
	public static Track FireEnergy  => MkEnergy( Element.Fire );
	public static Track PlantEnergy => MkEnergy( Element.Plant );
	public static Track MoonEnergy  => MkEnergy( Element.Moon );
	public static Track SunEnergy   => MkEnergy( Element.Sun );
	public static Track AirEnergy   => MkEnergy( Element.Air );
	public static Track AnyEnergy   => MkEnergy( Element.Any );
	public static Track AnimalEnergy=> MkEnergy( Element.Animal );
	public static Track EarthEnergy => MkEnergy( Element.Earth );
	public static Track WaterEnergy => MkEnergy( Element.Water );

	public static Track Card1 => MkCard(1);
	public static Track Card2 => MkCard(2);
	public static Track Card3 => MkCard(3);
	public static Track Card4 => MkCard(4);
	public static Track Card5 => MkCard(5);
	public static Track Card6 => MkCard(6);

	public static Track Push1Dahan => new Track( "Push1dahan" ){ 
		Action = new Push1DahanFromLands(),
		Icon = new IconDescriptor { BackgroundImg = Img.Land_Push_Dahan }
	};

	public static Track Push1TownCity => new Track( "Push1towncity" ) {
		Action = new Push1TownOrCityFromLands(),
		Icon = new IconDescriptor { BackgroundImg = Img.Land_Push_Town_City }
	};

	public static Track CardReclaim1 => new Track( "reclaim 1" ){ 
		Action=new ReclaimN(),
		Icon = new IconDescriptor { BackgroundImg = Img.Reclaim1 }
	};

	public static Track Energy5Reclaim1 => new Track( "5,reclaim1 energy" ) { 
		Energy = 5, Action = new ReclaimN(),
		Icon = new IconDescriptor { 
			BackgroundImg = Img.Coin, Text = "5",
			Sub = new IconDescriptor { BackgroundImg = Img.Reclaim1},
		}
	};


	public static Track Reclaim1Energy => new Track( "reclaim 1 energy" ){ 
		Action=new ReclaimN(),
		Icon = new IconDescriptor { BackgroundImg = Img.Coin, ContentImg = Img.Reclaim1 }
	};

	public static Track Card5Reclaim1 => new Track( "Five reclaim one" ){ 
		CardPlay=5, 
		Action = new ReclaimN(),
		Icon=new IconDescriptor { BackgroundImg = Img.CardPlay, Text="5", Sub = new IconDescriptor{ BackgroundImg = Img.Reclaim1 } },
	};

	public static readonly Track Destroyed = new Track("destroyed"); // only 1 of these

	public Track( string text, params Element[] els ){ this.Text = text; Elements = els; }

	public string Text {get;}

	public int? Energy { get; set; }

	public Element[] Elements { get; set; }

	public int? CardPlay { get; set; }

	/// <summary>
	/// If not null, this action is executed after Energy is collected.
	/// </summary>
	public IActOn<SelfCtx> Action { get; set; }

	public virtual void AddElement( CountDictionary<Element> elements ) {
		foreach(var el in Elements)
			elements[el]++;
	}

	public IconDescriptor Icon { get; set; }

}
