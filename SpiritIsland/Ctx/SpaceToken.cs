using SpiritIsland.Select;

namespace SpiritIsland;

public class SpaceToken : IOption {
	SpaceToken() { } // Null SpaceToken

	/// <param name="showSpace">If all of the tokens are on the same space, don't show it in the text.</param>
	public SpaceToken( Space space, IToken token, bool showSpace=true ) { 
		Space = space; 
		Token = token;
		Text = showSpace ? $"{Token.Text} on {space.Label}" : $"{Token.Text}";
	}

	public Space Space { get; }
	public IToken Token { get; }
	public string Text { get; }

	public void Deconstruct(out Space space, out IToken token) {
		space = Space;
		token = Token;
	}

	public Task<TokenMovedArgs> MoveTo(SpaceState destination) 
		=> Space.Tokens.MoveTo( Token, destination );

	public bool Exists => 0 < Space.Tokens[Token];

	public Task Destroy() => Space.Tokens.Destroy( Token, 1 );
	public Task Remove() => Space.Tokens.Remove( Token, 1 );


	public override int GetHashCode() => Space.GetHashCode()-Token.GetHashCode();
	public override bool Equals(object obj) => obj is SpaceToken st && st.Token == Token && st.Space == Space;
	// ! Don't make Record or include Text in hash code because sometimes we hide it.
}