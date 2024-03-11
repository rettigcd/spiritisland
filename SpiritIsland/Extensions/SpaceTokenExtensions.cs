namespace SpiritIsland;

static public class SpaceTokenExtensions {

	// Safe to Call from UI thread - does not rely on ActionScope.GameState to find Tokens
	public static SpaceToken On(this IToken token, SpaceState spaceState) => new SpaceToken(spaceState, token); // GOOD - captures tokens
	public static IEnumerable<SpaceToken> On( this IToken token, IEnumerable<SpaceState> spaces ) => spaces.Select(token.On);

	/// NOT SAFE to call from UI thread - relies on ActionScope.GameState to find Tokens
	public static IEnumerable<SpaceToken> OnScopeTokens1( this IEnumerable<IToken> tokens, Space space) => tokens.Select(t => t.OnScopeTokens(space));
	public static IEnumerable<SpaceToken> OnScopeTokens2( this IToken token, IEnumerable<Space> spaces ) => spaces.Select( token.OnScopeTokens );
	public static SpaceToken OnScopeTokens(this IToken token, Space space) => new SpaceToken(space.ScopeTokens, token);
}