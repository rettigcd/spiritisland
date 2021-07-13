using System;

namespace SpiritIsland.Core {

	public class DamagePlan : IOption {

		public DamagePlan(int damage,Invader invader){
			if( invader.Health < damage )
				throw new Exception("Damage exceeds health");

			Damage = damage;
			Invader = invader;
			DamagedInvader = invader.Damage(damage);
		}

		public readonly int Damage;
		public readonly Invader Invader;
		public readonly Invader DamagedInvader;

		public override string ToString() {
			return Damage + ">" + Invader.Summary;
		}

		string IOption.Text => ToString();

	}

	public class DamagePlanAction : DamagePlan {

		public DamagePlanAction(Space space,int damage,Invader invader):base(damage,invader){
			Space = space;
		}

		public readonly Space Space; // capture this so we can call .Apply
	}


}
