using System.Threading.Tasks;

namespace SpiritIsland {

	public class TargetSpiritCtx : SpiritGameStateCtx {

		public TargetSpiritCtx( Spirit self, GameState gs, Spirit target, Cause cause ) : base(self,gs,cause) {
			Other = target;
		}

		public Spirit Other { get; }

		public SpiritGameStateCtx OtherCtx => new SpiritGameStateCtx( Other, GameState, Cause, Originator );

	}

}