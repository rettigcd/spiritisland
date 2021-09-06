using System.Threading.Tasks;

namespace SpiritIsland {
	public class TargetSpiritCtx : IMakeGamestateDecisions {

		public TargetSpiritCtx(Spirit self,GameState gs,Spirit target ) {
			Self = self;
			GameState = gs;
			Target = target;
		}

		public Spirit Self { get; }

		public GameState GameState { get; }

		public Spirit Target { get; }


		public Task<Space> TargetSelectsPresenceLand(string prompt) {
			return Target.Action.Decide( new SelectDeployedPresence( 
				"Select location to remove blight and add presence", 
				Target 
			) );
		}

	}

}