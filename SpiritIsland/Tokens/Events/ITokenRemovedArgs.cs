namespace SpiritIsland;

public interface ITokenRemovedArgs {
	public Token Token { get; }
	public int Count { get; }
	public SpaceState RemovedFrom { get; }
	public RemoveReason Reason { get; }
	public UnitOfWork ActionScope { get; }
};
