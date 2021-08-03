
using System.Threading.Tasks;

namespace SpiritIsland.Core {

	public class GainEnergy : GrowthActionFactory {

		readonly int delta;
		public GainEnergy(int delta){
			this.delta = delta; 
		}

		public override string ShortDescription => $"GainEnergy({delta})";


		public override Task Activate(ActionEngine engine) {
			engine.Self.Energy += delta;
			return Task.CompletedTask;
		}

	}

}
