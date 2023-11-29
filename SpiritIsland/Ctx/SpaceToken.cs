using SpiritIsland.A;

namespace SpiritIsland;

public class SpaceToken : IOption {

	/// <param name="showSpaceInTextDescription">If all of the tokens are on the same space, don't show it in the text.</param>
	public SpaceToken( Space space, IToken token ) { 
		Space = space; 
		Token = token;
	}

	public Space Space { get; }
	public IToken Token { get; }
	public string Text => ShowSpaceInTextDescription ? $"{Token.Text} on {Space.Label}" : $"{Token.Text}";

	/// <summary> Set just before a Decision is made so text-interface only verbose when it needs to be. </summary>
	public bool ShowSpaceInTextDescription { get; set; }

	public void Deconstruct(out Space space, out IToken token) {
		space = Space;
		token = Token;
	}

	public Task<TokenMovedArgs> MoveTo(SpaceState destination) 
		=> Space.Tokens.MoveTo( Token, destination );

	public bool Exists => 0 < Count;
	public int Count => Space.Tokens[Token];

	public Task Destroy() => Space.Tokens.Destroy( Token, 1 );
	public Task Remove() => Space.Tokens.RemoveAsync( Token, 1 );


	public override int GetHashCode() => Space.GetHashCode()-Token.GetHashCode();
	public override bool Equals(object obj) => obj is SpaceToken st && st.Token == Token && st.Space == Space;
	// ! Don't make Record or include Text in hash code because sometimes we hide it.
}