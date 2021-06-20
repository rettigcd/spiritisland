using SpiritIsland.PowerCards;

namespace SpiritIsland {

	public abstract class GrowthAction : IActionFactory, IAction {

		protected Spirit spirit;
		protected GameState gameState;

		protected GrowthAction(){}

		public abstract void Apply();

		public IAction Bind(Spirit spirit, GameState gameState) {
			this.spirit = spirit;
			this.gameState = gameState;
			return this;
		}

		public virtual IOption[] Options => throw new System.NotImplementedException();

		public void Select(IOption option) {
			throw new System.NotImplementedException();
		}

		public abstract bool IsResolved {get;}

	}

}

// Option 1: Cards only work once they've been 'Bound'
// Option 2: Cards don't bind to user, wrapper does binding.
// Option 3: create 2 classes for each card
// Option 4: Self-shunt!

// !!!!!!    Cards are static
//      But when they are being played, they have instance variables.
// Therefore, we need separate classes for instance OR they need reset each play


