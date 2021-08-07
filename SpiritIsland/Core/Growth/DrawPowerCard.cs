using System.Threading.Tasks;

namespace SpiritIsland.Core {

	public class DrawPowerCard : GrowthActionFactory {

		public readonly int count;
		public DrawPowerCard(int count=1){ 
			this.count = count;
		}

		public override Task Activate( ActionEngine engine ) {
			engine.Self.PowerCardsToDraw += count;
			return Task.CompletedTask;
		}

	}

}
