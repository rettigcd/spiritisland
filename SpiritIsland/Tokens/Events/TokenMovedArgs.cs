namespace SpiritIsland;

public class TokenMovedArgs : ITokenMovedArgs {
	public GameState GameState { get; set; }
	public int Count { get; set; }

	public Token TokenRemoved { get; set; }
	public SpaceState RemovedFrom { get; set; }

	public SpaceState AddedTo { get; set; }
	public Token TokenAdded { get; set; }

	public UnitOfWork UnitOfWork { get; set; } // !!! Init this!

}


