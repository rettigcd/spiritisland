namespace SpiritIsland;

public class TokenType {
	static public readonly HealthTokenClass Dahan = new HealthTokenClass( "Dahan",  2, TokenCategory.Dahan, 0 );

	static public readonly UniqueToken Blight     = new UniqueToken( "Blight", 'B', Img.Blight ); // replace with a non-health type
	static public readonly UniqueToken Defend     = new UniqueToken( "Defend", 'G', Img.Defend ); // G:Guard D is for Dahan

	static readonly public UniqueToken Beast      = new UniqueToken("Beast",   'A', Img.Beast ); // need to use A for animal since B is already taken for blight
	static readonly public UniqueToken Wilds      = new UniqueToken("Wilds",   'W', Img.Wilds );
	static readonly public UniqueToken Disease    = new UniqueToken( "Dizease" ,'Z', Img.Disease );
	static readonly public UniqueToken Badlands   = new UniqueToken("Badlands",'M', Img.Badlands ); // 'M' looks like the badlands symbol /\/\ 

	static readonly public UniqueToken Isolate    = new UniqueToken( "Isolate", 'I', Img.Isolate );

	// Fake Tokens that are not visible.
	static readonly public UniqueToken DoBuild     = new UniqueToken( "Build", '$', Img.None );
	static readonly public UniqueToken Stasis      = new UniqueToken( "Stasis", '#', Img.None );

	static readonly public TokenClass OpenTheWays = new UniqueToken( "OpenTheWays", '=', Img.OpenTheWays );
	static readonly public TokenClass Element = new UniqueToken( "Element", 'Y', Img.Token_Any ); // use as unique class for stacked elements

}
