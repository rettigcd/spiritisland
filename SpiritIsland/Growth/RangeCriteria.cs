using System;

namespace SpiritIsland {

	public class RangeCriteria {

		static bool IsNotOcean(Space s) => s.Terrain != Terrain.Ocean;

		public RangeCriteria(int range){
			this.Range = range;
			this.IsValid = IsNotOcean;
		}

		public RangeCriteria(int range,Func<Space,bool> criteria){
			this.Range = range;
			this.IsValid = criteria ?? throw new ArgumentNullException(nameof(criteria));
		}

		public int Range {get;}
		public Func<Space,bool> IsValid {get;}

	}

}
