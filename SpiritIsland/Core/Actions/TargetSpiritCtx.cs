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
	}

}