namespace SpiritIsland;

public interface ITokenRemovedArgs {
	public Token Token { get; }
	public int Count { get; }
	public Space Space { get; }
	public RemoveReason Reason { get; }
	public GameState GameState { get; }
	public Guid ActionId { get; }
};
