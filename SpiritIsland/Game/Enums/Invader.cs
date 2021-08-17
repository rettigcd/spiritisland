using System.Collections.Generic;
using System.Linq;

namespace SpiritIsland {

	public class Invader {

		static public readonly Invader City = new Invader( "City", 4 ); // 0..3
		static public readonly Invader Town = new Invader( "Town", 3 );  // 0..2
		static public readonly Invader Explorer = new Invader( "Explorer", 2 );  // 0..1

		public Invader(string label, int length) { 
			Label = label;
			Specifics = new InvaderSpecific[length]; 
		}

		public string Label { get; }
		public InvaderSpecific[] Specifics {get;}


		public InvaderSpecific Healthy => Specifics[^1]; // we know healthy is last
		public InvaderSpecific Dead => Specifics[0]; // we know we put dead in first
		/// <summary> Starts at 1-hp, going up </summary>
		public IEnumerable<InvaderSpecific> AliveVariations => Specifics.Skip( 1 ); // not-dead variations
	}


	public class InvaderSpecific : IOption {

		// Healthy
		static readonly public InvaderSpecific Explorer0 = new InvaderSpecific( Invader.Explorer, 0);// DESTROYED
		static readonly public InvaderSpecific Explorer  = new InvaderSpecific( Invader.Explorer,1);
		static readonly public InvaderSpecific Town0     = new InvaderSpecific( Invader.Town,0); // DESTROYED
		static readonly public InvaderSpecific Town1     = new InvaderSpecific( Invader.Town,1); // damaged
		static readonly public InvaderSpecific Town      = new InvaderSpecific( Invader.Town,2); 
		static readonly public InvaderSpecific City0     = new InvaderSpecific( Invader.City,0); // DESTROYED
		static readonly public InvaderSpecific City1     = new InvaderSpecific( Invader.City,1); // damaged
		static readonly public InvaderSpecific City2     = new InvaderSpecific( Invader.City,2); // damaged
		static readonly public InvaderSpecific City      = new InvaderSpecific( Invader.City,3); 
		public const int TypesCount = 9;

		static readonly public Dictionary<string,InvaderSpecific> Lookup;

		static InvaderSpecific(){
			Lookup = Invader.City.Specifics
				.Union( Invader.Town.Specifics)
				.Union( Invader.Explorer.Specifics)
				.ToDictionary(i=>i.Summary);
		}

		#region private

		InvaderSpecific(Invader generic, int health){
			this.Generic = generic;
			Health = health;
			generic.Specifics[Health] = this;
		}

		public readonly Invader Generic;

		#endregion

		public string Summary => Initial+"@"+Health; // C@3, T@2
		public char Initial => Generic.Label[0];

		public InvaderSpecific Damage(int level){
			return Generic.Specifics[level > Health ? 0 : Health-level];
		}

		public int Health {get;}
		public InvaderSpecific Healthy => Generic.Healthy;
		public InvaderSpecific Dead => Generic.Dead;

		string IOption.Text =>  Summary; // + health ?
	}

}
