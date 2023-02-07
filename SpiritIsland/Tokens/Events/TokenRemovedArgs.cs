namespace SpiritIsland;

/// <summary>
/// Published (always has GameState)
/// </summary>
public class TokenRemovedArgs : ITokenRemovedArgs {

	public TokenRemovedArgs(IVisibleToken token, RemoveReason reason, SpaceState space, int count ) {
		Token = token;
		Reason = reason;
		RemovedFrom = space;
		Count = count;
	}
	public IVisibleToken Token { get; }
	public SpaceState RemovedFrom { get; }
	public int Count { get; }
	public RemoveReason Reason { get; }
}

