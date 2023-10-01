namespace SpiritIsland.Select;

public static class TokenFrom1Space {

	public static ASpaceToken TokenToPush( Space space, int count, IToken[] options, Present present )
		=> new ASpaceToken( present != Present.Done ? $"Push ({count})" : $"Push up to ({count})", space, options, present );

	public static ASpaceToken TokenToMove( Space srcSpace, int count, IToken[] options, Present present )
		=> new ASpaceToken( present != Present.Done ? $"Move ({count})" : $"Move up to ({count})", srcSpace, options, present );

	public static ASpaceToken TokenToRemove( Space space, int count, IToken[] options, Present present )
		=> new ASpaceToken( present != Present.Done ? $"Remove ({count})" : $"Remove up to ({count})", space, options, present );

}