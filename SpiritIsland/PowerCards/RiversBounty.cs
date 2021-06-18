namespace SpiritIsland.PowerCards
{

	[PowerCard("River's Bounty", 0, Speed.Slow,Element.Sun,Element.Water,Element.Animal)]
	public class RiversBounty : IAction {
		public RiversBounty(Spirit _){}

		public bool IsResolved => throw new System.NotImplementedException();

		public void Apply(){
			throw new System.NotImplementedException();
		}

		public IOption[] Options => throw new System.NotImplementedException();

		public void Select(IOption option) {
			throw new System.NotImplementedException();
		}

		// target: range 0 (any)

		// Gather up to 2 Dahan
		// If there are now at least 2 dahan, then add 1 dahan and gain 1 energy
	}

}



