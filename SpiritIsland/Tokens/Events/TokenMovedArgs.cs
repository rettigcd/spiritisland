namespace SpiritIsland;

public class TokenMovedArgs : ITokenMovedArgs {

	public TokenMovedArgs( SpaceState removedFrom, SpaceState addedTo ) {
		RemovedFrom = removedFrom;
		AddedTo = addedTo;
	}

	public SpaceState RemovedFrom { get; }
	public SpaceState AddedTo { get; }


	public int Count { get; set; }
	public ISpaceEntity TokenRemoved { get; set; } // sometimes different than TokenAdded due to Adding/Removing mods
	public ISpaceEntity TokenAdded { get; set; }

}


