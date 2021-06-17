using System;

namespace SpiritIsland {
	public class DamagePlan : IOption {
		public DamagePlan(int damage,Invader invader){
			if( invader.Health < damage )
				throw new Exception("Damage exceeds health");

			Damage = damage;
			Invader = invader;
		}

		public readonly int Damage;
		public readonly Invader Invader;

		public override string ToString() {
			return Damage + ">" + Invader.Summary;
		}

		string IOption.Text => ToString();
	}

}
