using System.Collections.Generic;
using System.Linq;

namespace SpiritIsland.Core {

	public class GrowthOption{

		public GrowthOption(params GrowthActionFactory[] actions){ 
			this.GrowthActions = actions; 
		}

		public GrowthOption(IEnumerable<GrowthActionFactory> actions){ 
			this.GrowthActions = actions.ToArray(); 
		}

		public GrowthActionFactory[] GrowthActions { get; }

		public override string ToString() {
			return GrowthActions.Select(a=>a.ShortDescription).Join(" / ");
		}

	}

}
