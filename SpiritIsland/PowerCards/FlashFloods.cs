namespace SpiritIsland.PowerCards {

	[PowerCard("Flash Floods",2,Speed.Fast,Element.Sun,Element.Water)]
	public class FlashFloods : IAction {

		public FlashFloods(Spirit _,GameState __){ }

		public bool IsResolved => throw new System.NotImplementedException();

		public void Apply() {
			throw new System.NotImplementedException();
		}

		// Target: range 1 (any)

		// +1 damage, if costal +1 additional damage
		public int GetDamage( Space space ){
			return space.IsCostal ? 2 : 1;
		}

	}

}