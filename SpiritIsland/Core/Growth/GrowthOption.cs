using System.Collections.Generic;
using System.Linq;

namespace SpiritIsland.Core {

	public class GrowthOption{

		public GrowthOption(params GrowthAction[] actions){ 
			this.GrowthActions = actions; 
		}

		public GrowthOption(IEnumerable<GrowthAction> actions){ 
			this.GrowthActions = actions.ToArray(); 
		}

		public GrowthAction[] GrowthActions { get; }

		public override string ToString() {
			return GrowthActions.Select(a=>a.ShortDescription).Join(" / ");
		}

	}

}
