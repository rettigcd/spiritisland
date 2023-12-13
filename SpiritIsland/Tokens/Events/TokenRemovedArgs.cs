namespace SpiritIsland;

/// <summary>
/// Published (always has GameState)
/// </summary>
public class TokenRemovedArgs : ITokenRemovedArgs {

	public TokenRemovedArgs(Space from, IToken token, int count, RemoveReason reason ) {
		Before = new SpaceToken(from,token);
		Count = count;
		Reason = reason;
	}
	public IToken Removed => Before.Token;
	public Space From => Before.Space;

	public SpaceToken Before { get; }
	public int Count { get; }
	public RemoveReason Reason { get; }
}

