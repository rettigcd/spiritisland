
using System.Threading.Tasks;

namespace SpiritIsland {

	public class GainEnergy : GrowthActionFactory {

		public int Delta { get; }

		public GainEnergy(int delta){
			this.Delta = delta; 
		}

		public override string Name => $"GainEnergy({Delta})";


		public override Task ActivateAsync( SpiritGameStateCtx ctx ) {
			ctx.Self.Energy += Delta;
			return Task.CompletedTask;
		}

	}

}
