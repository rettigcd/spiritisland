namespace SpiritIsland.Tests {
	public class FlashFloods : PowerCard{
		public FlashFloods():base("Flash Floods", 2, Speed.Fast, "SW"){}

		public int GetDamage( Space land ){
			return land.IsCostal ? 2 : 1;
		}

	}

}