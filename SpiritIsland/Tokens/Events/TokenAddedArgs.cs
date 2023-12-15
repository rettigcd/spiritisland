namespace SpiritIsland;

public class TokenAddedArgs : ITokenAddedArgs {

	public TokenAddedArgs(IToken token, ILocation to, int count, AddReason addReason ) {
		Added = token;
		To = to;
		Count = count;
		Reason = addReason;
	}

	public IToken Added { get; }
	public ILocation To { get; }

	public int Count { get; }

	public AddReason Reason { get; }

}
