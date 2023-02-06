namespace SpiritIsland;

public class UniqueToken : TokenClass, IVisibleToken, IAppearInSpaceAbreviation {

	public UniqueToken(string label, char initial, Img img, TokenCategory cat = TokenCategory.None) {
		Label = label;
		_summary = initial.ToString();
		Img = img;
		Category = cat;
	}

	/// <summary> Invisible token constructor </summary>
	public UniqueToken( string label, TokenCategory cat = TokenCategory.None ) {
		Label = label;
		_summary = "";			// invisible, does not appear in summary list
		Img = Img.None;	// invisible, does not appear on board
		Category = cat;
	}

	#region Token

	public TokenClass Class => this;

	public Img Img { get; }

	public override string ToString() => _summary;
	readonly string _summary;
	string IOption.Text => Label;

	public string SpaceAbreviation => _summary;

	#endregion

	#region TokenGroup

	public string Label { get; }

	public TokenCategory Category { get; }

	#endregion

}