namespace SpiritIsland {

	public class InvadersOnSpaceDecision : TypedDecision<Token> {
		public InvadersOnSpaceDecision( string prompt, Space space, Token[] options, Present present )
			: base( prompt, options, present ) { 
			Space = space;
		}
		public Space Space { get; }
	}

}