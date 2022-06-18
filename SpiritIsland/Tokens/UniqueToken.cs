namespace SpiritIsland;

public class UniqueToken : TokenClass, Token {

	public UniqueToken(string label, char initial, Img img, TokenCategory cat = TokenCategory.None) {
		this.Label = label;
		this.Initial = initial;
		this.Img = img;
		this.Category = cat;
	}

	#region Token

	public TokenClass Class => this;

	public Img Img { get; }

	public char Initial { get; }

	public override string ToString() => Initial.ToString();
//	string IOption.Text => Initial.ToString();
	string IOption.Text => Label;

	#endregion

	#region TokenGroup

	public string Label { get; }

	public TokenCategory Category { get; }

	#endregion

}