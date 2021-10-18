using System.Linq;

namespace SpiritIsland {

	public class GrowthOption : IOption {

		public int GainEnergy = 0; // When negative, prevents growth option unless user has sufficient energy to satisfy

		public GrowthOption(params GrowthActionFactory[] actions){ 
			this.GrowthActions = actions; 
		}

		public GrowthActionFactory[] GrowthActions { get; }

		public string Text => ToString();

		public override string ToString() {
			return GrowthActions.Select(a=>a.Name).Join(" / ");
		}

		public bool AutoSelectSingle { get;set; } = false;

	}

}
