using System.Threading.Tasks;

namespace SpiritIsland {
	public class PowerApiRestorer {

		readonly Spirit spirit;
		readonly TargetLandApi original;
		public PowerApiRestorer(Spirit spirit ) {
			this.spirit = spirit;
			this.original = spirit.TargetLandApi; // capture so we can put it back later
		}
		public Task Restore( GameState _ ) {
			spirit.TargetLandApi = original;
			return Task.CompletedTask;
		}

	}

}
