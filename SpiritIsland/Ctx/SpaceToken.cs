namespace SpiritIsland;

public record SpaceToken : IOption {

	public SpaceToken( Space space, Token token ) { 
		if(token is not IVisibleToken vt)
			throw new ArgumentException("SpaceToken is only for visible tokens, not "+token);
		Space = space; 
		Token = vt;
	}
	public Space Space { get; }
	public IVisibleToken Token { get; }

	public string Text => Token.ToString() + " on " + Space.Label;
}