namespace SpiritIsland;

public interface ITokenRemovedArgs {
	public IVisibleToken Token { get; }
	public int Count { get; }
	public SpaceState RemovedFrom { get; }
	public RemoveReason Reason { get; }
	public UnitOfWork ActionScope { get; }
};
