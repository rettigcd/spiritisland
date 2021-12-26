namespace SpiritIsland {

	public class InvadersRavaged {
		public Space Space;

		public int damageFromAttackers;
		public int dahanDestroyed;
		public int startingDahan;
		public int damageFromDefenders;
		public string endingInvaders;
		public string startingInvaders;

		public override string ToString() {
			string s = $"{Space.Label}: Invaders dealt {damageFromAttackers} damage to {startingDahan} dahan destroying {dahanDestroyed} of them.";
			if(damageFromDefenders>0)
				s += $"  Dahan fight back dealing {damageFromDefenders} damage, leaving {endingInvaders}";
			return s;
		}

	}


}
