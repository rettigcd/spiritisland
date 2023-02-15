namespace SpiritIsland;

public interface ITokenRemovedArgs {
	public IToken Removed { get; }
	public int Count { get; }
	public SpaceState From { get; }
	public RemoveReason Reason { get; }
};
