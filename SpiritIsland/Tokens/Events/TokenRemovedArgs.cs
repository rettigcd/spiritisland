namespace SpiritIsland;

/// <summary>
/// Published (always has GameState)
/// </summary>
public class TokenRemovedArgs : ITokenRemovedArgs {

	public TokenRemovedArgs(ILocation from, IToken token, int count, RemoveReason reason ) {
		From = from;
		Removed = token;
		Count = count;
		Reason = reason;
	}
	public IToken Removed { get; }
	public ILocation From { get; }

	public int Count { get; }
	public RemoveReason Reason { get; }
}
