using System.Collections.Generic;
using System.Linq;

namespace SpiritIsland.Core {

	public class GrowthOption : IOption {

		public GrowthOption(params GrowthActionFactory[] actions){ 
			this.GrowthActions = actions; 
		}

		public GrowthActionFactory[] GrowthActions { get; }

		public string Text => ToString();

		public override string ToString() {
			return GrowthActions.Select(a=>a.ShortDescription).Join(" / ");
		}

	}

}
