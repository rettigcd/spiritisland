
namespace SpiritIsland {

	public class InvaderSpecific : IOption {

		#region private

		public InvaderSpecific(Invader generic, InvaderSpecific[] seq, int health){
			this.Generic = generic;
			Health = health;
			this.seq = seq;
		}

		public readonly Invader Generic;

		#endregion

		public virtual string Summary => Initial+"@"+Health; // C@3, T@2
		public char Initial => Generic.Label[0];

		public InvaderSpecific Damage(int level){
			return seq[level > Health ? 0 : Health-level];
		}

		public int Health {get;}

		public InvaderSpecific Healthy => seq[^1];

		public int FullHealth => Healthy.Health;

		string IOption.Text =>  Summary; // + health ?

		readonly InvaderSpecific[] seq;
	}

}
