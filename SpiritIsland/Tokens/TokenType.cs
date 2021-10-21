namespace SpiritIsland {

	public class TokenType {
		static public readonly TokenGroup Dahan = new TokenGroup( "Dahan",  2, 'D' );
		static public readonly Token Blight     = new TokenGroup( "Blight", 1, 'B' ).Default; // replace with a non-health type
		static public readonly Token Defend     = new TokenGroup( "Defend", 1, 'G' ).Default; // G:Guard D is for Dahan

		static readonly public Token Beast      = new TokenGroup("Beast",   1, 'A').Default; // need to use A for animal since B is already taken for blight
		static readonly public Token Wilds      = new TokenGroup("Wilds",   1, 'W').Default;
		static readonly public Token Disease    = new TokenGroup("Dizease" ,1, 'Z').Default;

		static readonly public Token Isolate    = new TokenGroup("Isolate" ,1, 'I').Default;
		static readonly public Token Badlands   = new TokenGroup("Badlands",1, 'M').Default; // 'M' looks like the badlands symbol /\/\ 
	}

}
