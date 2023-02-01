namespace SpiritIsland;

public class TokenAddedArgs : ITokenAddedArgs {

	public TokenAddedArgs(ActionableSpaceState space, IToken token, AddReason addReason, int count, UnitOfWork actionScope ) {
		AddedTo = space;
		Token = token;
		Reason = addReason;
		Count = count;
		ActionScope = actionScope;
	}

	public IToken Token { get; } // need specific so we can act on it (push/damage/destroy)
	public ActionableSpaceState AddedTo { get; }

	public int Count { get; }
	public AddReason Reason { get; }
	public UnitOfWork ActionScope { get; }

	public GameState GameState { get; set; }

}
