namespace SpiritIsland.Tests {
	public class BoonOfVigor : PowerCard{
		public BoonOfVigor():base("Boon of Vigor", 0, Speed.Fast, "SWP"){}

		public int TargetSelf(){
			return 1;
		}

		public int TargetOther(int powerCardsPlayed){
			return powerCardsPlayed;
		}

	}

}