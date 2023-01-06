namespace SpiritIsland;

public interface ITokenMovedArgs {
	int Count { get; }

	Token TokenRemoved { get; }
	SpaceState RemovedFrom { get; }

	Token TokenAdded { get; }
	SpaceState AddedTo { get; }

	UnitOfWork ActionScope { get; }
}

