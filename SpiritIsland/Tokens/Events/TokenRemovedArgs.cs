namespace SpiritIsland;

/// <summary>
/// Published (always has GameState)
/// </summary>
public class TokenRemovedArgs : ITokenRemovedArgs {

	public TokenRemovedArgs(IToken token, RemoveReason reason, SpaceState space, int count ) {
		Removed = token;
		Reason = reason;
		From = space;
		Count = count;
	}
	public IToken Removed { get; }
	public SpaceState From { get; }
	public int Count { get; }
	public RemoveReason Reason { get; }
}

