using System.Threading.Tasks;

namespace SpiritIsland {

	public class TargetSpiritCtx : PowerCtx {

		public TargetSpiritCtx(Spirit self,GameState gs,Spirit target ) : base(self,gs) {
			Target = target;
		}

		public Spirit Target { get; }

		public SpiritGameStateCtx TargetCtx => Target.MakeDecisionsFor(GameState);

		public async Task<TargetSpaceCtx> TargetSelectsPresenceLand(string prompt) {
			var space = await Target.Action.Decide( new SelectDeployedPresence( 
				"Select location to remove blight and add presence", 
				Target 
			) );
			return new TargetSpaceCtx(Target,GameState,space);
		}

	}

}