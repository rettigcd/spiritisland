namespace SpiritIsland;

/// <summary>
/// Published (always has GameState)
/// </summary>
public class TokenRemovedArgs : ITokenRemovedArgs {

	public TokenRemovedArgs(SpaceState space, IToken token, int count, RemoveReason reason ) {
		Removed = token;
		From = space;

		Before = new SpaceToken(space.Space,token);

		Count = count;

		Reason = reason;
	}
	public IToken Removed { get; }
	public SpaceState From { get; }

	public SpaceToken Before { get; }

	public int Count { get; }
	public RemoveReason Reason { get; }
}

