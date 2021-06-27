namespace SpiritIsland.PowerCards {
	public abstract class TargetSpiritAction : BaseAction {

		protected readonly Spirit self;

		public TargetSpiritAction(Spirit spirit, GameState gameState):base(gameState){
			this.self = spirit;
			engine.decisions.Push(new TargetSpirit(gameState.Spirits,SelectSpirit));
		}

		protected abstract void SelectSpirit(Spirit spirit);
	}

}