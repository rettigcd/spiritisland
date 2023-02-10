namespace SpiritIsland;

public interface ITokenRemovedArgs {
	public IToken Token { get; }
	public int Count { get; }
	public SpaceState RemovedFrom { get; }
	public RemoveReason Reason { get; }
};
