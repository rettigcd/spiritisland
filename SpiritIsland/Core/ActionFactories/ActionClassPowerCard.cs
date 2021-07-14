using System;
using System.Linq;

namespace SpiritIsland.Core {

	/// <summary>
	/// Power Card implemented by a a class implementing IAction
	/// </summary>
	class ActionClassPowerCard : PowerCard {
		public ActionClassPowerCard(Type actionType) {
			PowerCardAttribute[] pca = FindPowerCardAttributes( actionType );
			if(pca.Length == 0) throw new ArgumentException( actionType.Name + " missing PowerCard attribute." );
			if(pca.Length != 1) throw new ArgumentException( actionType.Name + " has multiple PowerCard attributes." );
			var attr = pca[0];

			Speed = attr.Speed;
			Name = attr.Name;
			Cost = attr.Cost;
			Elements = attr.Elements;
			PowerType = attr.PowerType;

			this.actionType = actionType;
		}

		static public PowerCardAttribute[] FindPowerCardAttributes( Type actionType ) {
			return Attribute.GetCustomAttributes( actionType )
				.OfType<PowerCardAttribute>()
				.ToArray();
		}

		readonly Type actionType;

		public override IAction Bind(Spirit spirit,GameState gameState){
			return (IAction)Activator.CreateInstance(actionType,spirit,gameState);
		}

		public override string ToString() => Name;
	}

}