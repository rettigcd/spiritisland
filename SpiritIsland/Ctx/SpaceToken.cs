namespace SpiritIsland;

public class SpaceToken : IOption {

	/// <param name="showSpace">If all of the tokens are on the same space, don't show it in the text.</param>
	public SpaceToken( Space space, IVisibleToken token, bool showSpace=true ) { 
		Space = space; 
		Token = token;
		Text = showSpace ? $"{Token} on {space.Label}" : $"{Token}";
	}

	public Space Space { get; }
	public IVisibleToken Token { get; }
	public string Text { get; }

	public void Deconstruct(out Space space, out IVisibleToken token) {
		space = Space;
		token = Token;
	}

	public override int GetHashCode() => Space.GetHashCode()-Token.GetHashCode();
	public override bool Equals(object obj) => obj is SpaceToken st && st.Token == Token && st.Space == Space;
	// ! Don't make Record or include Text in hash code because sometimes we hide it.
}