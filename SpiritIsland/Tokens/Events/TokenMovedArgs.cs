namespace SpiritIsland;

public class TokenMovedArgs : ITokenMovedArgs {
	public TokenClass Class { get; set; }
	public Token Token { get; set;}
	public int Count { get; set; }
	public GameState GameState { get; set; }

	public Space RemovedFrom { get; set; }
	public Space AddedTo { get; set; }

	public Guid ActionId { get; set; } // !!! Init this!

}


