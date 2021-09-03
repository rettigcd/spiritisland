using System.Collections.Generic;

namespace SpiritIsland {

	public class InvadersOnSpaceDecision : TypedDecision<Token> {
		public InvadersOnSpaceDecision( string prompt, Space space, IEnumerable<Token> options, Present present )
			: base( prompt, options, present ) { 
			Space = space;
		}
		public Space Space { get; }
	}

	public class AddStrifeDecision : InvadersOnSpaceDecision {
		public AddStrifeDecision( TokenCountDictionary tokens )
			: base( "Add Strife", tokens.Space, tokens.Invaders(), Present.Always ) { }
	}

}