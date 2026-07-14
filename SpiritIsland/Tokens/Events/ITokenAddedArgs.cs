namespace SpiritIsland;

public interface ITokenAddedArgs {

	public IToken Added { get; } // need specific so we can act on it (push/damage/destroy)
	public ILocation To { get; }

	public int Count { get; }
	public AddReason Reason { get; }
}

