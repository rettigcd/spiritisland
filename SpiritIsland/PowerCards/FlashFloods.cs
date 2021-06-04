namespace SpiritIsland.Tests {
	public class FlashFloods : PowerCard{
		public FlashFloods():base("Flash Floods", 2, Speed.Fast, "SW"){}

		public int GetDamage( Space space ){
			return space.IsCostal ? 2 : 1;
		}

	}

}