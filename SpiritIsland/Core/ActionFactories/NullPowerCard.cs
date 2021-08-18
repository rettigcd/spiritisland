using System;
using System.Threading.Tasks;

namespace SpiritIsland {
	public class NullPowerCard : PowerCard {
		public NullPowerCard(string name, int cost, Speed speed, params Element[] elements){
			this.Name = name;
			this.Cost = cost;
			this.Speed = speed;
			this.Elements = elements;
			this.PowerType = PowerType.Spirit; // HACK!
		}

		public override Task Activate( Spirit _, GameState _1 ) {return Task.CompletedTask;}

	}

}