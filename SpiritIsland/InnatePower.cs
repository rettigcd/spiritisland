using System;

namespace SpiritIsland {
	public class InnatePower : IActionFactory, IAction {
		public Speed Speed { get; protected set; }
		public string Name { get; protected set; }

		public void Resolved(Spirit spirit){
			spirit.UnresolvedActionFactories.Remove(this);
		}

		public bool IsResolved => true;

		public IOption[] Options => new IOption[0];

		public void Apply() {
		}

		public IAction Bind(Spirit spirit, GameState gameState) {
			return this;
		}

		public InnateAction GetAction(Spirit _){
//			var action = (IAction)Activator.CreateInstance(actionType,spirit);
//			return new CardAction(this,action); // wrap it
			return null;
		}

		public void Select( IOption option ) {
			throw new NotImplementedException();
		}
	}

}