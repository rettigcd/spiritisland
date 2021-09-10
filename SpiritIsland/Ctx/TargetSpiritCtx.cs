using System.Threading.Tasks;

namespace SpiritIsland {

	public class TargetSpiritCtx : PowerCtx {

		public TargetSpiritCtx( Spirit self, GameState gs, Spirit target ) : base(self,gs) {
			Other = target;
		}

		public Spirit Other { get; }

		public PowerCtx OtherCtx => new PowerCtx( Other, GameState );

	}

}