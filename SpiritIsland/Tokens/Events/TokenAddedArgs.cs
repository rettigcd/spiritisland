namespace SpiritIsland;

public class TokenAddedArgs : ITokenAddedArgs {

	public TokenAddedArgs(SpaceState space, IToken token, int count, AddReason addReason ) {
		To = space;
		Added = token;
		After = new SpaceToken(space.Space,token);

		Count = count;
		Reason = addReason;
	}

	public IToken Added { get; }
	public SpaceState To { get; }
	public SpaceToken After { get; }

	public int Count { get; }

	public AddReason Reason { get; }

}
