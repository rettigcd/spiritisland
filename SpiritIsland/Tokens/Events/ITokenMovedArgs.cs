namespace SpiritIsland;

public interface ITokenMovedArgs {
	GameState GameState { get; }

	int Count { get; }

	Token TokenRemoved { get; }
	SpaceState RemovedFrom { get; }

	Token TokenAdded { get; }
	SpaceState AddedTo { get; }

	UnitOfWork UnitOfWork { get; }
}

