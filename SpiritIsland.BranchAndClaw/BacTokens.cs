namespace SpiritIsland.BranchAndClaw {
	public static class BacTokens {
		static readonly public Token Beast = new TokenGroup("Beast",1,'A').Default; // need to use A for animal since B is already taken for blight
		static readonly public Token Wilds = new TokenGroup("Wilds",1).Default;
		static readonly public Token Disease = new TokenGroup("Zizease",1).Default;
	}


	public static class ExtendTokens {
		static public TokenBinding Beasts( this TokenCountDictionary counts ) => new ( counts, BacTokens.Beast );
		static public TokenBinding Disease( this TokenCountDictionary counts ) => new ( counts, BacTokens.Disease );
		static public TokenBinding Wilds( this TokenCountDictionary counts ) => new ( counts, BacTokens.Wilds );
	}


}
