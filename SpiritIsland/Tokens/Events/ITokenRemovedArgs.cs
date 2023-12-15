namespace SpiritIsland;

public interface ITokenRemovedArgs {

	public IToken Removed { get; }
	public ILocation From { get; }

	public int Count { get; }

	public RemoveReason Reason { get; }
};
