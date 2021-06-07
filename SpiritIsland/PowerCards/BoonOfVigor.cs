namespace SpiritIsland.PowerCards {

	public class BoonOfVigor : PowerCard{

		public BoonOfVigor():base("Boon of Vigor", 0, Speed.Fast
			, Element.Sun
			, Element.Water
			, Element.Plant
		){}

		// Target: any spirit

		public int TargetSelf(){ // opt 1
			return 1; // spirit gains 1 energy
		}

		public int TargetOther(int powerCardsPlayed){ // opt 2
			return powerCardsPlayed; // target spirit ganes 1 energy per power card played
		}

	}

}