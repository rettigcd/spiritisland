using System.Collections.Generic;
using System.Linq;

namespace SpiritIsland {

	public class Invader : IOption {

		static readonly Invader[] Cities = new Invader[4]; // 0..3
		static readonly Invader[] Towns = new Invader[3];  // 0..2
		static readonly Invader[] Explorers = new Invader[2];  // 0..1

		// Healthy
		static readonly public Invader Explorer0 = new Invader( 0, "Explorer",Explorers,0);// DESTROYED
		static readonly public Invader Explorer  = new Invader( 1, "Explorer",Explorers,1);
		static readonly public Invader Town0     = new Invader( 2, "Town",Towns,0); // DESTROYED
		static readonly public Invader Town1     = new Invader( 3, "Town",Towns,1); // damaged
		static readonly public Invader Town      = new Invader( 4, "Town",Towns,2); 
		static readonly public Invader City0     = new Invader( 5, "City",Cities,0); // DESTROYED
		static readonly public Invader City1     = new Invader( 6, "City",Cities,1); // damaged
		static readonly public Invader City2     = new Invader( 7, "City",Cities,2); // damaged
		static readonly public Invader City      = new Invader( 8, "City",Cities,3); 
		public const int TypesCount = 9;

		static readonly public Dictionary<string,Invader> Lookup;

		static Invader(){
			Lookup = Cities.Union(Towns).Union(Explorers).ToDictionary(i=>i.Summary);
		}

		#region private

		Invader(int index, string label, Invader[] typeArr, int health){
			Index = index;
			Label = label;
			this.typeArr = typeArr;
			Health = health;
			typeArr[Health] = this;
		}

		readonly Invader[] typeArr;

		#endregion

		public int Index { get; }

		public IEnumerable<Invader> AliveVariations => typeArr.Skip(1); // not-dead variations

		public string Summary => Initial+"@"+Health; // C@3, T@2

		public Invader Damage(int level){
			return typeArr[level > Health ? 0 : Health-level];
		}

		public int Health {get;}
		public Invader Healthy => typeArr[^1];
		public Invader Dead => typeArr[0];

		public char Initial => Label[0];

		public string Label { get; }

		string IOption.Text =>  Summary; // + health ?
	}

}
