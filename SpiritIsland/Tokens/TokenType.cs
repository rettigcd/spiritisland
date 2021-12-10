namespace SpiritIsland {

	public class TokenType {
		static public readonly TokenGroup Dahan = new TokenGroup( "Dahan",  2, 'D', Img.Dahan1, Img.Dahan2 );
		static public readonly Token Blight     = new TokenGroup( "Blight", 1, 'B', Img.Blight ).Default; // replace with a non-health type
		static public readonly Token Defend     = new TokenGroup( "Defend", 1, 'G', Img.Defend ).Default; // G:Guard D is for Dahan

		static readonly public Token Beast      = new TokenGroup("Beast",   1, 'A', Img.Beast ).Default; // need to use A for animal since B is already taken for blight
		static readonly public Token Wilds      = new TokenGroup("Wilds",   1, 'W', Img.Wilds ).Default;
		static readonly public Token Disease    = new TokenGroup("Dizease" ,1, 'Z', Img.Disease ).Default;

		static readonly public Token Isolate    = new TokenGroup("Isolate" ,1, 'I', Img.Isolate ).Default;
		static readonly public Token Badlands   = new TokenGroup("Badlands",1, 'M', Img.Badlands ).Default; // 'M' looks like the badlands symbol /\/\ 
	}

}
