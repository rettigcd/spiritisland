namespace SpiritIsland;

/// <summary>
/// Published (always has GameState)
/// </summary>
public class TokenRemovedArgs : ITokenRemovedArgs {

	public TokenRemovedArgs(IToken token, RemoveReason reason, SpaceState space, int count ) {
		Token = token;
		Reason = reason;
		RemovedFrom = space;
		Count = count;
	}
	public IToken Token { get; }
	public SpaceState RemovedFrom { get; }
	public int Count { get; }
	public RemoveReason Reason { get; }
}

