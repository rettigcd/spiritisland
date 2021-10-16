using System.Linq;

namespace SpiritIsland {

	public class GrowthOption : IOption {

		public int GainEnergy = 0; // indicator to limit option availability

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
