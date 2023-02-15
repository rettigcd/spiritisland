namespace SpiritIsland;

public class TokenAddedArgs : ITokenAddedArgs {

	public TokenAddedArgs(SpaceState space, IToken token, AddReason addReason, int count ) {
		To = space;
		Added = token;
		Reason = addReason;
		Count = count;
	}

	public IToken Added { get; }
	public SpaceState To { get; }

	public int Count { get; }
	public AddReason Reason { get; }

}
