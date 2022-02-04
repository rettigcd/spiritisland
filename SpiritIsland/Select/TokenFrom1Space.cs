namespace SpiritIsland.Select;

// Do NOT merge this into SpaceTokens because that adds the additional complexity of ignoring the Space that is returned.
public class TokenFrom1Space : TypedDecision<Token> {

	public static TokenFrom1Space TokenToPush( SpiritIsland.Space space, int count, Token[] options, Present present )
		=> new TokenFrom1Space( present != Present.Done ? $"Push ({count})" : $"Push up to ({count})", space, options, present );

	public static TokenFrom1Space TokenToRemove( SpiritIsland.Space space, int count, Token[] options, Present present )
		=> new TokenFrom1Space( present != Present.Done ? $"Remove ({count})" : $"Remove up to ({count})", space, options, present );

	public TokenFrom1Space( string prompt, SpiritIsland.Space space, IEnumerable<Token> options, Present present  )
		: base( prompt, options, present ) 
	{ 
		Space = space;
	}

	public SpiritIsland.Space Space { get; }

	// In theory, we could add IHaveAdjacentInfo && AdjacentInfo
	// but nothing seems to need it.

}