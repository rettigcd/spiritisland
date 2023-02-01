namespace SpiritIsland;

public class TokenMovedArgs : ITokenMovedArgs {

	public TokenMovedArgs( SpaceState removedFrom, SpaceState addedTo, UnitOfWork actionScope ) {
		ActionScope = actionScope;
		RemovedFrom = removedFrom;
		AddedTo = addedTo;
	}

	public int Count { get; set; }

	public IToken TokenRemoved { get; set; } // sometimes different than TokenAdded due to Adding/Removing mods
	public SpaceState RemovedFrom { get; }

	public IToken TokenAdded { get; set; }
	public SpaceState AddedTo { get; }

	public UnitOfWork ActionScope { get; }

}


