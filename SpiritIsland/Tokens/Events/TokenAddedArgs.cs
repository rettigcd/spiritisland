namespace SpiritIsland;

public class TokenAddedArgs : ITokenAddedArgs {

	public TokenAddedArgs(Space space, Token token, AddReason addReason, int count, Guid actionId ) {
		Space = space;
		Token = token;
		Reason = addReason;
		Count = count;
		ActionId = actionId;
	}

	public Token Token { get; } // need specific so we can act on it (push/damage/destroy)
	public Space Space { get; }
	public int Count { get; }
	public AddReason Reason { get; }
	public Guid ActionId { get; }

	public GameState GameState { get; set; }

}
