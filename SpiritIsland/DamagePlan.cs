using SpiritIsland.PowerCards;
using System;

namespace SpiritIsland {

	public class DamagePlan : IOption, IAtomicAction {

		public DamagePlan(Space space, int damage,Invader invader){
			if( invader.Health < damage )
				throw new Exception("Damage exceeds health");

			this.Space = space;
			Damage = damage;
			Invader = invader;
			DamagedInvader = invader.Damage(damage);
		}

		public readonly Space Space; // capture this so we can call .Apply

		public readonly int Damage;
		public readonly Invader Invader;

		public readonly Invader DamagedInvader;

		public override string ToString() {
			return Damage + ">" + Invader.Summary;
		}

		void IAtomicAction.Apply( GameState gameState ) {
			gameState.ApplyDamage(this);
		}

		string IOption.Text => ToString();
	}

}
