namespace SpiritIsland;

public class TokenAddedArgs : ITokenAddedArgs {

	public TokenAddedArgs(SpaceState space, IToken token, int count, AddReason addReason ) {
		After = new SpaceToken(space.Space,token);
		Count = count;
		Reason = addReason;
	}

	public SpaceToken After { get; }

	public IToken Added => After.Token;
	public Space To => After.Space;

	public int Count { get; }

	public AddReason Reason { get; }

}
