namespace SpiritIsland;

static public class SpaceTokenExtensions {

	// Safe to Call from UI thread - does not rely on ActionScope.GameState to find Tokens

	// 1:1
	public static SpaceToken On(this IToken token, Space space) => new SpaceToken(space, token); // GOOD - captures tokens

	// 1:*
	public static IEnumerable<SpaceToken> On( this IToken token, IEnumerable<Space> spaces ) => spaces.Select(token.On);

	// *:1
	public static IEnumerable<SpaceToken> On(this IEnumerable<IToken> tokens, Space space) => tokens.Select(t => t.On(space));

}