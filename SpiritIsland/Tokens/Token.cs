namespace SpiritIsland;

public static class Token {

	static public readonly UniqueToken Blight   = new BlightToken ( "Blight",  'B', Img.Blight ); // replace with a non-health type

	// BaseGame
	static public readonly UniqueToken Defend   = new UniqueToken ( "Defend",  'G', Img.Defend ); // G:Guard D is for Dahan
	static readonly public UniqueToken Isolate  = new UniqueToken ( "Isolate", 'I', Img.Isolate );

	// Branch and Claw
	static readonly public UniqueToken Beast   = new UniqueToken( "Beast", 'A', Img.Beast ); // need to use A for animal since B is already taken for blight
	static readonly public UniqueToken Wilds   = new WildsToken( "Wilds", 'W', Img.Wilds );
	static readonly public UniqueToken Disease = new DiseaseToken( "Disease", 'Z', Img.Disease );

	// Jagged Earth
	static readonly public UniqueToken Badlands = new UniqueToken ( "Badlands",'M', Img.Badlands ); // 'M' looks like the badlands symbol /\/\ 
	static readonly public UniqueToken Element = new UniqueToken( "Element", 'Y', Img.Token_Any ); // use as unique class for stacked elements
	static readonly public TokenClass OpenTheWays = new UniqueToken( "OpenTheWays", '=', Img.OpenTheWays );

}

public static class ModToken {
	// Fake Tokens that are not visible.
	static readonly public InvisibleToken DoExplore = new InvisibleToken( "Explore" );
	static readonly public InvisibleToken DoBuild = new InvisibleToken( "Build" );
	static readonly public InvisibleToken DoRavage = new InvisibleToken( "Ravage" );
}