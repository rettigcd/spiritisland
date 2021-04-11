namespace SpiritIsland.Tests {
	public class FlashFloods : PowerCard{

		public override int Cost => 2;
		public override Speed Speed => Speed.Fast;
		public override string Elements => "SW";

		public int GetDamage( BoardSpace land ){
			return land.IsCostal ? 2 : 1;
		}

	}

}