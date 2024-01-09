namespace SpiritIsland;

static public class SpaceTokenExtensions {
	public static SpaceToken On( this IToken token, Space space ) => new SpaceToken( space, token );

	/// <summary>Shows the Space in the SpaceToken's description.</summary>
	public static IEnumerable<SpaceToken> On( this IEnumerable<IToken> tokens, Space space ) 
		=> tokens.Select( t => t.On( space ) );

	/// <summary>Convenience method.  Downgrades space-states to spaces.</summary>
	public static IEnumerable<SpaceToken> On( this IToken token, IEnumerable<SpaceState> spaces ) => token.On( spaces.Downgrade() );
	public static IEnumerable<SpaceToken> On( this IToken token, IEnumerable<Space> spaces ) => spaces.Select( space => token.On( space ) );

}