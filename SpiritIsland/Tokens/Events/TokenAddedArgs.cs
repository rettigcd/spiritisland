namespace SpiritIsland;

public class TokenAddedArgs : ITokenAddedArgs {

	public TokenAddedArgs(ActionableSpaceState space, IVisibleToken token, AddReason addReason, int count ) {
		AddedTo = space;
		Token = token;
		Reason = addReason;
		Count = count;
	}

	public IVisibleToken Token { get; }
	public ActionableSpaceState AddedTo { get; }

	public int Count { get; }
	public AddReason Reason { get; }

	public GameState GameState { get; set; }

}
