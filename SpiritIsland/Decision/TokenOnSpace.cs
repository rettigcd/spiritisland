﻿using System.Collections.Generic;

namespace SpiritIsland.Decision {

	public class TokenOnSpace : TypedDecision<Token> {
		public TokenOnSpace( string prompt, Space space, IEnumerable<Token> options, Present present )
			: base( prompt, options, present ) { 
			Space = space;
		}
		public Space Space { get; }
	}

}