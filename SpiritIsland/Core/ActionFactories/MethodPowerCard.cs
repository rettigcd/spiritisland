using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace SpiritIsland.Core {
	class MethodPowerCard : PowerCard {
		readonly Func<ActionEngine,Spirit,GameState,Task> actionType;
		public MethodPowerCard(Func<ActionEngine,Spirit,GameState,Task> actionType){
			MethodBase m = System.Reflection.RuntimeReflectionExtensions.GetMethodInfo( actionType );
			var pca = m.GetCustomAttributes<PowerCardAttribute>().ToArray();
			if(pca.Length == 0) throw new ArgumentException( m.Name + " missing PowerCard attribute." );
			if(pca.Length != 1) throw new ArgumentException( m.Name + " has multiple PowerCard attributes." );
			var attr = pca[0];

			Speed = attr.Speed;
			Name = attr.Name;
			Cost = attr.Cost;
			Elements = attr.Elements;
			PowerType = attr.PowerType;
			this.actionType = actionType;
		}

		public override IAction Bind( Spirit spirit, GameState gameState ) {
			return new MethodBaseAction(spirit,gameState,actionType);
		}

		class MethodBaseAction : BaseAction {
			public MethodBaseAction(
				Spirit spirit, 
				GameState gameState,Func<ActionEngine,Spirit,GameState,Task> actionType
			):base(gameState)
			{
				_ = actionType(engine,spirit,gameState);
			}
		}
	}

}