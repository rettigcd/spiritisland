namespace SpiritIsland.Select;

/// <summary>
/// Similar to SpaceToken except Space is known.  Therefore:
/// (1) We don't have to keep presenting it to the user:  "T2 on A5" vs "T2"  (This is the real pain in the ass part...)
/// (2) We don't have to explicitly pick out the Token piece of the returned value.  token   vs    token.Token
/// So.. Do NOT merge this into SpaceTokens because that adds the additional complexity of ignoring the Space that is returned.
/// (Twice I tried to merge this into SpaceTokens, and BOTH times when I got to the Unit Tests, I decided to roll it back.)
/// </summary>
public class TokenFrom1Space : TypedDecision<SpaceToken> {

	public static TokenFrom1Space TokenToPush( SpiritIsland.Space space, int count, IVisibleToken[] options, Present present )
		=> new TokenFrom1Space( present != Present.Done ? $"Push ({count})" : $"Push up to ({count})", space, options, present );

	public static TokenFrom1Space TokenToMove( SpiritIsland.Space srcSpace, int count, IVisibleToken[] options, Present present )
		=> new TokenFrom1Space( present != Present.Done ? $"Move ({count})" : $"Move up to ({count})", srcSpace, options, present );

	public static TokenFrom1Space TokenToRemove( SpiritIsland.Space space, int count, IVisibleToken[] options, Present present )
		=> new TokenFrom1Space( present != Present.Done ? $"Remove ({count})" : $"Remove up to ({count})", space, options, present );

	public TokenFrom1Space( string prompt, SpiritIsland.Space space, IEnumerable<IVisibleToken> options, Present present  )
		: base( prompt, options.Select(t=>new SpaceToken(space,t,false)), present ) 
	{ 
		Space = space;
	}

	public SpiritIsland.Space Space { get; }

	// TokenFromManySpaces

}