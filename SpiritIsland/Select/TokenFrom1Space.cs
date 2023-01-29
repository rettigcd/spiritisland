namespace SpiritIsland.Select;

public static class TokenFrom1Space {

	public static TokenFromManySpaces TokenToPush( SpiritIsland.Space space, int count, IVisibleToken[] options, Present present )
		=> new TokenFromManySpaces( present != Present.Done ? $"Push ({count})" : $"Push up to ({count})", space, options, present );

	public static TokenFromManySpaces TokenToMove( SpiritIsland.Space srcSpace, int count, IVisibleToken[] options, Present present )
		=> new TokenFromManySpaces( present != Present.Done ? $"Move ({count})" : $"Move up to ({count})", srcSpace, options, present );

	public static TokenFromManySpaces TokenToRemove( SpiritIsland.Space space, int count, IVisibleToken[] options, Present present )
		=> new TokenFromManySpaces( present != Present.Done ? $"Remove ({count})" : $"Remove up to ({count})", space, options, present );

}