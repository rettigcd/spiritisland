
namespace SpiritIsland {

	public class Token : IOption {

		#region private

		public Token(TokenGroup generic, Token[] seq, int health){
			this.Generic = generic;
			Health = health;
			this.seq = seq;
		}

		public readonly TokenGroup Generic;

		#endregion

		public virtual string Summary => Initial+"@"+Health; // C@3, T@2

		public char Initial => Generic.Initial;

		public Token ResultingDamagedInvader(int level){
			return seq[ level > Health ? 0 : Health-level ];
		}

		public int Health {get;}

		public Token Healthy => seq[^1];

		public int FullHealth => Healthy.Health;

		string IOption.Text =>  Summary; // + health ?

		readonly Token[] seq;
	}

}
