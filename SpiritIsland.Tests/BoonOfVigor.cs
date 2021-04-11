namespace SpiritIsland.Tests {
	public class BoonOfVigor : PowerCard{

		public override int Cost => 0;
		public override Speed Speed => Speed.Fast;
		public override string Elements => "SWP";

		public int TargetSelf(){
			return 1;
		}

		public int TargetOther(int powerCardsPlayed){
			return powerCardsPlayed;
		}

	}

}