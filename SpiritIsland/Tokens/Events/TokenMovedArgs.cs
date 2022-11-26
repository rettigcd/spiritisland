namespace SpiritIsland;

public class TokenMovedArgs : ITokenMovedArgs {
	public TokenClass Class { get; set; }
	public Token Token { get; set;}
	public int Count { get; set; }
	public GameState GameState { get; set; }

	public SpaceState RemovedFrom { get; set; }
	public SpaceState AddedTo { get; set; }

	public UnitOfWork ActionId { get; set; } // !!! Init this!

}


