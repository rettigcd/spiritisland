namespace SpiritIsland;

public class TokenAddedArgs( IToken token, ILocation to, int count, AddReason addReason ) 
	: ITokenAddedArgs
{
	public IToken Added { get; } = token;
	public ILocation To { get; } = to;

	public int Count { get; } = count;

	public AddReason Reason { get; } = addReason;

}
