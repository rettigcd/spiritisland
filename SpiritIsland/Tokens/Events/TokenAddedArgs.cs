namespace SpiritIsland;

public class TokenAddedArgs : ITokenAddedArgs {

	public TokenAddedArgs(SpaceState space, Token token, AddReason addReason, int count, UnitOfWork actionScope ) {
		AddedTo = space;
		Token = token;
		Reason = addReason;
		Count = count;
		ActionScope = actionScope;
	}

	public Token Token { get; } // need specific so we can act on it (push/damage/destroy)
	public SpaceState AddedTo { get; }

	public int Count { get; }
	public AddReason Reason { get; }
	public UnitOfWork ActionScope { get; }

	public GameState GameState { get; set; }

}
