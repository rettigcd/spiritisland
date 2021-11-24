﻿using System.Collections.Generic;

namespace SpiritIsland.Decision {

	// For Selecting Token from single space
	// !! This could be merged into SpaceTokens
	public class TokenOnSpace : TypedDecision<Token> {

		public static TokenOnSpace TokenToPush( Space space, int count, Token[] options, Present present )
			=> new TokenOnSpace( present != Present.Done ? $"Push ({count})" : $"Push up to ({count})", space, options, present );

		public static TokenOnSpace InvaderToDowngrade( Space space, IEnumerable<Token> options, Present present ) 
			=> new TokenOnSpace( "Select invader to down-grade (C=>T or T=>E)", space, options, present );

		public static TokenOnSpace InvaderToRemove( Space space, IEnumerable<Token> options, Present present ) 
			=> new TokenOnSpace( "Remove invader", space, options, present );

		public TokenOnSpace( 
			string prompt, 
			Space space, IEnumerable<Token> options, 
			Present present 
		)
			: base( prompt, options, present ) 
		{ 
			Space = space;
		}
		public Space Space { get; }
	}


}