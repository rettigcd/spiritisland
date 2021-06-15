using SpiritIsland.PowerCards;
using System;
using System.Linq;

namespace SpiritIsland {

	public interface IActionFactory {
		IAction Bind(Spirit spirit,GameState gameState);
	}


	public class PowerCard : IActionFactory {

		static public PowerCard For<T>(){ return new PowerCard(typeof(T));}

		public PowerCard(Type actionType){

			var pca = System.Attribute.GetCustomAttributes(actionType)
				.OfType<PowerCardAttribute>()
				.ToArray();
			if(pca.Length==0) throw new ArgumentException(actionType.Name+" missing PowerCard attribute.");
			if(pca.Length!=1) throw new ArgumentException(actionType.Name+" has multiple PowerCard attributes.");
			var attr = pca[0];

			this.Name = attr.Name;
			this.Cost = attr.Cost;
			this.Speed = attr.Speed;
			this.Elements = attr.Elements;

			this.actionType = actionType;
		}

		public PowerCard(string name, int cost, Speed speed, params Element[] elements){
			this.Name = name;
			this.Cost = cost;
			this.Speed = speed;
			this.Elements = elements;
			this.actionType = typeof(NullCardAction);
		}

		public string Name { get; }
		public int Cost { get; }
		public Speed Speed { get; }
		public Element[] Elements { get; }

		readonly Type actionType;

		public IAction Bind(Spirit spirit,GameState gameState){
			return (IAction)Activator.CreateInstance(actionType,spirit,gameState);
		}

		class NullCardAction : IAction {
			public NullCardAction(Spirit _){ }
			public bool IsResolved => true;
			public void Apply(){}

			public IOption[] GetOptions() { return new IOption[0]; }

			public void Select(IOption option) {}
		}

	}

	public class InnatePower : IActionFactory {
		public Speed Speed { get; }

		public IAction Bind(Spirit spirit, GameState gameState) {
			return null;
		}

		public InnateAction GetAction(Spirit _){
//			var action = (IAction)Activator.CreateInstance(actionType,spirit);
//			return new CardAction(this,action); // wrap it
			return null;
		}

	}

}