using System.Collections.Generic;
using System.Linq;

namespace SpiritIsland {
	public class DrawCardResult {
		public DrawCardResult( PowerType powerType ) {
			this.PowerType = powerType;
		}
		public PowerType PowerType { get; }
		public PowerCard[] SelectedCards;
		public PowerCard Selected => SelectedCards.Single();
		public List<PowerCard> Rejected;
	}

}