
using System.Threading.Tasks;

namespace SpiritIsland {

	public class GainEnergy : GrowthActionFactory {

		readonly int delta;
		public GainEnergy(int delta){
			this.delta = delta; 
		}

		public override string Name => $"GainEnergy({delta})";


		public override Task ActivateAsync( SpiritGameStateCtx ctx ) {
			ctx.Self.Energy += delta;
			return Task.CompletedTask;
		}

	}

}
