﻿
namespace SpiritIsland {

	public class HealthToken : Token {
		public HealthToken( TokenClass generic, Token[] seq, int health, Img img ) {
			this.Class = generic;
			Health = health;
			this.seq = seq;
			this.Img = img;
		}

		public TokenClass Class { get; } // originally: readonly

		public Img Img { get; }

		public virtual string Summary => Initial+"@"+Health; // originally: virtual

		public char Initial => Class.Initial;

		public Token ResultingDamagedInvader(int damage){
			return seq[ damage > Health ? 0 : Health-damage ];
		}

		public int Health {get;}

		public Token Healthy => seq[^1];

		public int FullHealth => Healthy.Health;

		string IOption.Text =>  Summary;

		readonly Token[] seq;


	}

}
