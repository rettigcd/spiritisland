namespace SpiritIsland.PowerCards {
	public class FlashFloods : PowerCard{
		public FlashFloods():base("Flash Floods", 2, Speed.Fast
			, Element.Sun
			, Element.Water
		){}

		// Target: range 1 (any)

		// +1 damage, if costal +1 additional damage
		public int GetDamage( Space space ){
			return space.IsCostal ? 2 : 1;
		}

	}

}