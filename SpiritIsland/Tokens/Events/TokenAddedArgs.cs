namespace SpiritIsland;

public class TokenAddedArgs : ITokenAddedArgs {

	public TokenAddedArgs(SpaceState space, IVisibleToken token, AddReason addReason, int count ) {
		AddedTo = space;
		Token = token;
		Reason = addReason;
		Count = count;
	}

	public IVisibleToken Token { get; }
	public SpaceState AddedTo { get; }

	public int Count { get; }
	public AddReason Reason { get; }

}
