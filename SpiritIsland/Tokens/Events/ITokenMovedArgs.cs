namespace SpiritIsland;

public interface ITokenMovedArgs {
	GameState GameState { get; }

	TokenClass Class { get; }
	int Count { get; }
	Space RemovedFrom { get; }
	SpaceState AddedTo { get; }
	Guid ActionId { get; }
}

