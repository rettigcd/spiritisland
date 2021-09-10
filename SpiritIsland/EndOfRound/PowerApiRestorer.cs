using System.Threading.Tasks;

namespace SpiritIsland {
	public class PowerApiRestorer {
		Spirit spirit;
		TargetLandApi original;
		public PowerApiRestorer(Spirit spirit ) {
			this.spirit = spirit;
			this.original = spirit.PowerApi; // capture so we can put it back later
		}
		public Task Restore( GameState _ ) {
			spirit.PowerApi = original;
			return Task.CompletedTask;
		}
	}

}
