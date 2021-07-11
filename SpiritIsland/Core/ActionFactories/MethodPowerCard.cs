using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace SpiritIsland.Core {
	class MethodPowerCard : PowerCard {
//		readonly Func<ActionEngine,Spirit,GameState,Task> actionType;
		readonly MethodBase m;
		public MethodPowerCard(MethodBase m){
			var pca = m.GetCustomAttributes<PowerCardAttribute>().ToArray();
			if(pca.Length == 0) throw new ArgumentException( m.Name + " missing PowerCard attribute." );
			if(pca.Length != 1) throw new ArgumentException( m.Name + " has multiple PowerCard attributes." );
			var attr = pca[0];

			Speed = attr.Speed;
			Name = attr.Name;
			Cost = attr.Cost;
			Elements = attr.Elements;
			PowerType = attr.PowerType;
//			this.actionType = actionType;
			this.m = m;
		}

		public override IAction Bind( Spirit spirit, GameState gameState ) {
			return new MethodBaseAction(spirit,gameState,m);
		}

		class MethodBaseAction : BaseAction {
			public MethodBaseAction(
				Spirit spirit,
				GameState gameState,
				MethodBase m
			):base(spirit,gameState)
			{
				m.Invoke(null,new object[]{engine});
			}
		}
	}

}