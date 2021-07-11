using System;
using System.Threading.Tasks;

namespace SpiritIsland.Core {

	public abstract class PowerCard : IActionFactory, IOption {

		static public PowerCard For<T>(){ return new ActionClassPowerCard(typeof(T));}

		static public PowerCard For( Func<ActionEngine, Spirit, GameState, Task> actionType ) {
			return new MethodPowerCard(actionType);
		}

		public string Name { get; protected set; }
		public int Cost { get; protected set;  }
		public Speed Speed { get; protected set;  }
		public PowerCard Original => this;
		public Element[] Elements { get; protected set;  }
		public PowerType PowerType { get; protected set;  }
		IActionFactory IActionFactory.Original => this;

		public string Text => Name;

		abstract public IAction Bind(Spirit spirit,GameState gameState);

	}

}