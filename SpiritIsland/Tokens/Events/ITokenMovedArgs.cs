namespace SpiritIsland;

public interface ITokenMovedArgs {
	int Count { get; }

	IToken TokenRemoved { get; }
	SpaceState RemovedFrom { get; }

	IToken TokenAdded { get; }
	SpaceState AddedTo { get; }
}

