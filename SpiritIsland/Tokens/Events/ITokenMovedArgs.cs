namespace SpiritIsland;

public interface ITokenMovedArgs {
	GameState GameState { get; }

	TokenClass Class { get; }
	int Count { get; }
	Space RemovedFrom { get; }
	Space AddedTo { get; }
	Guid ActionId { get; }
}

