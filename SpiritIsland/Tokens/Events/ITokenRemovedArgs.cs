namespace SpiritIsland;

public interface ITokenRemovedArgs {

	public IToken Removed { get; }
	public Space From { get; }

	/// <summary> The token type and space before it was removed. </summary>
	public SpaceToken Before { get; }

	public int Count { get; }

	public RemoveReason Reason { get; }
};
