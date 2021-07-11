using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace SpiritIsland.Core {

	public abstract class PowerCard : IActionFactory, IOption {

		static public PowerCard For<T>(){
			// Try Class first
			Type type = typeof(T);
			var pca = ActionClassPowerCard.FindPowerCardAttributes(type);
			if(pca.Length == 1)
				return new ActionClassPowerCard(type);

			// try static method
			var methods = type.GetMethods(BindingFlags.Public|BindingFlags.Static)
				.Where(m=>m.GetCustomAttributes<PowerCardAttribute>().Count()==1)
				.ToArray();
			return methods.Length == 1 
				? new MethodPowerCard(methods[0]) 
				: throw new ArgumentException("Invalid PowerCard class "+type.Name);
		}

		//static public PowerCard For( Func<ActionEngine, Spirit, GameState, Task> actionType ) {
		//			MethodBase m = System.Reflection.RuntimeReflectionExtensions.GetMethodInfo( actionType );
		//	return new MethodPowerCard(m);
		//}

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