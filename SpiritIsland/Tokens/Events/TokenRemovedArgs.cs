namespace SpiritIsland;

/// <summary>
/// Published (always has GameState)
/// </summary>
public class TokenRemovedArgs( ILocation from, IToken token, int count, RemoveReason reason ) : ITokenRemovedArgs {
	public IToken Removed { get; } = token;
	public ILocation From { get; } = from;

	public int Count { get; } = count;
	public RemoveReason Reason { get; } = reason;
}
