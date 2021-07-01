using System;
using System.Linq;

namespace SpiritIsland.Core {

	public class PowerCard : IActionFactory, IOption {

		static public PowerCard For<T>(){ return new PowerCard(typeof(T));}

		public PowerCard(Type actionType){

			var pca = System.Attribute.GetCustomAttributes(actionType)
				.OfType<PowerCardAttribute>()
				.ToArray();
			if(pca.Length==0) throw new ArgumentException(actionType.Name+" missing PowerCard attribute.");
			if(pca.Length!=1) throw new ArgumentException(actionType.Name+" has multiple PowerCard attributes.");
			var attr = pca[0];

			Speed = attr.Speed;
			Name = attr.Name;
			Cost = attr.Cost;
			Elements = attr.Elements;

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

		public string Text => Name;

		readonly Type actionType;

		public IAction Bind(Spirit spirit,GameState gameState){
			return (IAction)Activator.CreateInstance(actionType,spirit,gameState);
		}

		public void Resolved(Spirit spirit){
			spirit.UnresolvedActionFactories.Remove(this);
		}


		class NullCardAction : IAction {
			public NullCardAction(Spirit _){ }
			public bool IsResolved => true;
			public void Apply(){}

			public IOption[] Options => Array.Empty<IOption>();

			public string Prompt => throw new NotImplementedException();

			public void Select(IOption option) {}
		}

		public override string ToString() => Name;

	}

}