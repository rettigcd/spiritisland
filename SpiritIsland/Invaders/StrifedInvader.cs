namespace SpiritIsland {

	public partial class StrifedInvader : HealthToken {

		static internal readonly StrifedInvaderGenerator Generator = new();

		public StrifedInvader(TokenCategory generic,Token[] seq,int health, int strifeCount)
			:base( generic, seq, health, generic[health].Img ) {
			StrifeCount = strifeCount;
		}
		public override string Summary => base.Summary + new string( '^', StrifeCount);
		public int StrifeCount { get; }

	}

}
