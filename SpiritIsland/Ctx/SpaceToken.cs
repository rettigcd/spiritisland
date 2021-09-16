namespace SpiritIsland {
	public class SpaceToken : IOption {
		public SpaceToken( Space space, Token token ) { Space = space; Token = token; }
		public Space Space { get; }
		public Token Token { get; }

		public string Text => Token.Summary + " on " + Space.Label;
	}

}