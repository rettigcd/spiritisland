namespace SpiritIsland;

public static class Token {

	static public readonly TokenClassToken Blight   = new BlightToken ( "Blight",  'B', Img.Blight ); // replace with a non-health type

	// BaseGame
	static public readonly TokenClassToken Defend   = new TokenClassToken ( "Defend",  'G', Img.Defend ); // G:Guard D is for Dahan
	static readonly public TokenClassToken Isolate  = new IsolateToken( "Isolate", 'I', Img.Isolate );

	// Branch and Claw
	static readonly public TokenClassToken Beast   = new TokenClassToken( "Beast", 'A', Img.Beast ); // need to use A for animal since B is already taken for blight
	static readonly public TokenClassToken Wilds   = new WildsToken( "Wilds", 'W', Img.Wilds );
	static readonly public DiseaseToken Disease    = new DiseaseToken();

	// Jagged Earth
	static readonly public TokenClassToken Badlands = new TokenClassToken ( "Badlands",'M', Img.Badlands ); // 'M' looks like the badlands symbol /\/\ 
	static readonly public TokenClassToken Element = new TokenClassToken( "Element", 'Y', Img.Token_Any ); // use as unique class for stacked elements
	static readonly public ITokenClass OpenTheWays = new TokenClassToken( "OpenTheWays", '=', Img.OpenTheWays );

	// Nature Incarnate
	static readonly public TokenClassToken Vitality = new VitalityToken("Vitality", 'V', Img.Vitality );
	static readonly public TokenClassToken Quake    = new TokenClassToken( "Quake", 'Q', Img.Quake );
}

public static class ModToken {
	// Fake Tokens that are not visible.
	static readonly public InvaderActionToken DoExplore = new InvaderActionToken( "Explore" );
	static readonly public InvaderActionToken DoBuild = new InvaderActionToken( "Build" );
	static readonly public InvaderActionToken DoRavage = new InvaderActionToken( "Ravage" );
}
