namespace SpiritIsland;

public class TokenAddedArgs : ITokenAddedArgs {

	public TokenAddedArgs(SpaceState space, Token token, AddReason addReason, int count, UnitOfWork actionId ) {
		Space = space;
		Token = token;
		Reason = addReason;
		Count = count;
		Action = actionId;
	}

	public Token Token { get; } // need specific so we can act on it (push/damage/destroy)
	public SpaceState Space { get; }

	public int Count { get; }
	public AddReason Reason { get; }
	public UnitOfWork Action { get; }

	public GameState GameState { get; set; }

}
