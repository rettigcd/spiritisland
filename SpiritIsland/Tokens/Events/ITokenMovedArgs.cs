namespace SpiritIsland;

public interface ITokenMovedArgs {
	int Count { get; }

	ISpaceEntity TokenRemoved { get; }
	SpaceState RemovedFrom { get; }

	ISpaceEntity TokenAdded { get; }
	SpaceState AddedTo { get; }
}

