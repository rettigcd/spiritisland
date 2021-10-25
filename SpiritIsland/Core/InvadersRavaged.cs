namespace SpiritIsland {

	public class InvadersRavaged {
		public Space Space;

		public int dahanDamageFromInvaders;
		public int dahanDestroyed;
		public int dahanOnSpace;
		public int damageFromDahan;
		public string endingInvaders;
		public string startingInvaders;

		public override string ToString() {
			string s = $"{Space.Label}: Invaders dealt {dahanDamageFromInvaders} damage to {dahanOnSpace} dahan destroying {dahanDestroyed} of them.";
			if(damageFromDahan>0)
				s += "  Dahan fight back dealing {damageFromDahan} damage, leaving {endingInvaders}";
			return s;
		}

	}


}
