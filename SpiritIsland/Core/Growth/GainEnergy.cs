
using System.Threading.Tasks;

namespace SpiritIsland {

	public class GainEnergy : GrowthActionFactory {

		readonly int delta;
		public GainEnergy(int delta){
			this.delta = delta; 
		}

		public override string ShortDescription => $"GainEnergy({delta})";


		public override Task ActivateAsync( Spirit spirit, GameState _) {
			spirit.Energy += delta;
			return Task.CompletedTask;
		}

	}

}
