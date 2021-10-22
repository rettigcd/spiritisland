﻿using System.Linq;
using System.Threading.Tasks;
using Elem = SpiritIsland.Element;

namespace SpiritIsland {

	public class Track : IOption {

		public static Track MkEnergy( int energy, Element el ) => new Track( energy+","+ el.ToString().ToLower() + " energy", el ) { Energy = energy };
		public static Track MkEnergy( int energy ) => new Track( energy + " energy" ) { Energy = energy };
		public static Track MkEnergy(params Element[] els ) => new Track( els.Select(x=>x.ToString()).Join(",").ToLower() + " energy", els );

		public static Track MkCard(int plays) => new Track($"{plays} cardplay" ) { CardPlay = plays };
		public static Track MkElement(params Element[] els) => new Track( els.Select(x=>x.ToString()).Join(",").ToLower(), els );

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
		public static readonly Track AnimalEnergy= MkEnergy( Elem.Animal );
		public static readonly Track EarthEnergy = MkEnergy( Elem.Earth );
		public static readonly Track WaterEnergy = MkEnergy( Elem.Water );

		public static readonly Track Card1 = MkCard(1);
		public static readonly Track Card2 = MkCard(2);
		public static readonly Track Card3 = MkCard(3);
		public static readonly Track Card4 = MkCard(4);
		public static readonly Track Card5 = MkCard(5);
		public static readonly Track Card6 = MkCard(6);

		public static readonly Track Push1Dahan = new Track( "Push1dahan" ){ Action = new Push1DahanFromLands() };
		public static readonly Track Reclaim1 = new Track( "reclaim 1" ){ Action=new Reclaim1() };
		public static readonly Track Reclaim1Energy = new Track( "reclaim 1 energy" ){ Action=new Reclaim1() };
		public static readonly Track Energy5Reclaim1 = new Track( "5,reclaim1 energy" ){ Energy=5, Action=new Reclaim1() };
		public static readonly Track Card5Reclaim1 = new Track( "Fivereclaimone" ){ CardPlay=5, Action=new Reclaim1() };
		public static readonly Track Destroyed = new Track("destroyed"); 

		public Track( string text, params Element[] els ){ this.Text = text; Elements = els; }

		public string Text {get;}

		public int? Energy { get; private set; }

		public Element[] Elements { get; }

		public int? CardPlay { get; set; }

		public IActionFactory Action { get; set; }

		public virtual void AddElement( CountDictionary<Element> elements ) {
			foreach(var el in Elements)
				elements[el]++;
		}

	}

	/// <summary> Pushes 1 dahan from 1 of your lands. </summary>
	public class Push1DahanFromLands : GrowthActionFactory {
		public override async Task ActivateAsync( SpiritGameStateCtx ctx ) {
			var dahanOptions = ctx.Self.Presence.Spaces
				.SelectMany(space=>ctx.Target(space).Dahan.Keys.Select(t=>new SpaceToken(space,t)));
//				.SelectMany(space=>ctx.Target(space).Tokens.OfType(TokenType.Dahan).Select(t=>new SpaceToken(space,t)));
			var source = await ctx.Self.Action.Decision(new Decision.SpaceTokens("Select dahan to push from land",dahanOptions,Present.Done));
			if(source == null) return;

			await new TokenPusher( ctx, source.Space ).PushToken( source.Token );
		}
	}

}
