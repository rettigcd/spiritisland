namespace SpiritIsland.PowerCards {

	[PowerCard("Boon of Vigor", 0, Speed.Fast,Element.Sun,Element.Water,Element.Plant)]
	public class BoonOfVigor : IAction{

		public BoonOfVigor(Spirit _){}

		// Target: any spirit

		public int TargetSelf(){ // opt 1
			return 1; // spirit gains 1 energy
		}

		public int TargetOther(int powerCardsPlayed){ // opt 2
			return powerCardsPlayed; // target spirit ganes 1 energy per power card played
		}

		public bool IsResolved => throw new System.NotImplementedException();

		public void Apply() {
			throw new System.NotImplementedException();
		}
	}

}