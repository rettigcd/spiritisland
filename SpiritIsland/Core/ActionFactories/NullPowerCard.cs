using System;

namespace SpiritIsland.Core {
	public class NullPowerCard : PowerCard {
		public NullPowerCard(string name, int cost, Speed speed, params Element[] elements){
			this.Name = name;
			this.Cost = cost;
			this.Speed = speed;
			this.Elements = elements;
			this.PowerType = PowerType.Spirit; // HACK!
		}
		public override IAction Bind( Spirit spirit, GameState gameState ) {
			return new NullCardAction();
		}

		class NullCardAction : IAction {
			public NullCardAction(){ }
			public bool IsResolved => true;
			public void Apply(){}

			public IOption[] Options => Array.Empty<IOption>();

			public string Prompt => throw new NotImplementedException();

			public string Selections => "no action";

			public void Select(IOption option) {}
		}

	}

}