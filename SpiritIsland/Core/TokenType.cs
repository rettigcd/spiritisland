namespace SpiritIsland {

	public class TokenType {
		static public readonly TokenGroup Dahan = new TokenGroup( "Dahan", 2 );
		static public readonly Token Blight = new TokenGroup( "Blight", 1 ).Default; // replace with a non-health type
		static public readonly Token Defend = new TokenGroup( "Defend", 1, 'G' ).Default; // G:Guard D is for Dahan
	}

}
