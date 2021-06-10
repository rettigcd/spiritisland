namespace SpiritIsland.PowerCards {

	[PowerCard("Wash Away", 1, Speed.Slow, Element.Water, Element.Earth)]
	public class WashAway : IAction {

		public WashAway(Spirit _){}

		public bool IsResolved => throw new System.NotImplementedException();

		public void Apply() {
			throw new System.NotImplementedException();
		}

		// target: range 1

		// push up to 3 explorers / towns

	}
}
