using System;

namespace SpiritIsland {
	public class DamagePlan : IOption {
		public DamagePlan(int damage,Invader invader,int health){
			if( MaxInvaderHealth(invader) < health )
				throw new Exception("There are no "+invader+" with health "+health);
			if( health < damage )
				throw new Exception("Damage exceeds health");

			Damage = damage;
			Invader = invader;
			Health = health;
		}

		static int MaxInvaderHealth(Invader invader ) => invader switch {
			Invader.City => 3,
			Invader.Town => 2,
			Invader.Explorer => 1,
			_ => 0
		};

		public readonly int Damage;
		public readonly Invader Invader;
		public readonly int Health;

		public string InvaderHealth => InvaderInitial+"@"+Health; // C@3, T@2
		public override string ToString() {
			return Damage + ">" + InvaderHealth;
		}

		string InvaderInitial => Invader switch{
			Invader.City => "C",
			Invader.Town => "T",
			Invader.Explorer => "E",
			_ => throw new System.Exception()
		};

		string IOption.Text => ToString();
	}

	public enum Invader { City, Town, Explorer }

}
