﻿namespace SpiritIsland {

	public partial class StrifedInvader : Token {

		static internal readonly StrifedInvaderGenerator Generator = new();

		public StrifedInvader(TokenGroup generic,Token[] seq,int health, int strifeCount)
			:base( generic, seq, health ) {
			StrifeCount = strifeCount;
		}
		public override string Summary => base.Summary + new string( '^', StrifeCount);
		public int StrifeCount { get; }

	}

}
