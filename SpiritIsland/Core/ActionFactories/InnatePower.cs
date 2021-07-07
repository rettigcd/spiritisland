using System;
using System.Linq;

namespace SpiritIsland.Core {

	public class InnatePower : IActionFactory {

		#region Constructors and factories

		static public InnatePower For<T>(){ return new InnatePower(typeof(T));}

		public InnatePower(Type actionType){

			var attributes = System.Attribute.GetCustomAttributes(actionType);
			var pca = attributes
				.OfType<InnatePowerAttribute>()
				.ToArray();
			if(pca.Length==0) throw new ArgumentException(actionType.Name+" missing InnatePower attribute.");
			if(pca.Length!=1) throw new ArgumentException(actionType.Name+" has multiple InnatePower attributes.");
			var attr = pca[0];

			Speed = attr.Speed;
			Name = attr.Name;

			powerLevels = attributes
				.OfType<PowerLevelAttribute>()
				.ToArray();

			this.actionType = actionType;
		}

		#endregion

		public int PowersActivated(Spirit spirit){
			bool[] isSubSet = powerLevels
				.Select(pl=>spirit.HasElements(pl.Elements))
				.ToArray();

			return isSubSet.Count(x=>x);
		}

		public Speed Speed {get;}

		public string Name {get;}

		public string Text => Name;

		public IActionFactory Original => this;

		readonly PowerLevelAttribute[] powerLevels;

		readonly Type actionType;

		public IAction Bind( Spirit spirit, GameState gameState ) {
			return (IAction)Activator.CreateInstance(actionType,spirit,gameState);
		}

	}

}