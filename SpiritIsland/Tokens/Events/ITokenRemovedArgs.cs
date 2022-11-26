namespace SpiritIsland;

public interface ITokenRemovedArgs {
	public Token Token { get; }
	public int Count { get; }
	public SpaceState Space { get; }
	public RemoveReason Reason { get; }
	public GameState GameState { get; }
	public UnitOfWork ActionId { get; }
};
