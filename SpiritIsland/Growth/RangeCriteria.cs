using System;

namespace SpiritIsland {

	public class RangeCriteria {

		public RangeCriteria(int range){
			this.Range = range;
			this.IsValid = ((s)=>s.Terrain != Terrain.Ocean);
		}

		public RangeCriteria(int range,Func<Space,bool> criteria){
			this.Range = range;
			this.IsValid = criteria ?? throw new ArgumentNullException(nameof(criteria));
		}

		public int Range {get;}
		public Func<Space,bool> IsValid {get;}

	}

}
