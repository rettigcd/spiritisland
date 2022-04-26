namespace SpiritIsland;

public class UniqueToken : TokenClass, Token {

	public UniqueToken(string label, char initial, Img img, TokenCategory cat = TokenCategory.None) {
		this.Label = label;
		this.Img = img;
		this.Initial = initial;
		this.Category = cat;
	}


	#region Token

	public TokenClass Class => this;

	public Img Img { get; }

	public char Initial { get; }

	public override string ToString() => Initial.ToString();
	string IOption.Text => Initial.ToString();

	// --------  BEGIN Token HEALTH properties
	public Token Healthy => throw new System.NotImplementedException();
	public int FullHealth => throw new System.NotImplementedException();
	public int RemainingHealth => 1; //throw new System.NotImplementedException();
	// --------  END HEALTH properties  -------

	#endregion

	#region TokenGroup

	public string Label { get; }

	public TokenCategory Category { get; }

	#endregion

}