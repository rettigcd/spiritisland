namespace SpiritIsland;

public interface ITokenMovedArgs {
	GameState GameState { get; }

	TokenClass Class { get; }
	int Count { get; }
	SpaceState RemovedFrom { get; }
	SpaceState AddedTo { get; }
	UnitOfWork ActionId { get; }
}

