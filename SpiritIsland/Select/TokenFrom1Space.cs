namespace SpiritIsland.Select;

// Do NOT merge this into SpaceTokens because that adds the additional complexity of ignoring the Space that is returned.
public class TokenFrom1Space : TypedDecision<Token> {

	public static TokenFrom1Space TokenToPush( SpiritIsland.Space space, int count, Token[] options, Present present )
		=> new TokenFrom1Space( present != Present.Done ? $"Push ({count})" : $"Push up to ({count})", space, options, present );

	public static TokenFrom1Space TokenToMove( SpiritIsland.Space srcSpace, int count, Token[] options, Present present )
		=> new TokenFrom1Space( present != Present.Done ? $"Move ({count})" : $"Move up to ({count})", srcSpace, options, present );

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

public class HealthTokenFrom1Space : TypedDecision<HealthToken> {

	//public static HealthTokenFrom1Space TokenToPush( SpiritIsland.Space space, int count, HealthToken[] options, Present present )
	//	=> new HealthTokenFrom1Space( present != Present.Done ? $"Push ({count})" : $"Push up to ({count})", space, options, present );

	//public static HealthTokenFrom1Space TokenToRemove( SpiritIsland.Space space, int count, HealthToken[] options, Present present )
	//	=> new HealthTokenFrom1Space( present != Present.Done ? $"Remove ({count})" : $"Remove up to ({count})", space, options, present );

	public HealthTokenFrom1Space( string prompt, SpiritIsland.Space space, IEnumerable<HealthToken> options, Present present )
		: base( prompt, options, present ) {
		Space = space;
	}

	public SpiritIsland.Space Space { get; }

}