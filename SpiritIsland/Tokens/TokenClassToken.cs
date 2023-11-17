namespace SpiritIsland;

/// <summary>
/// Token that implements its own IEntity.Class
/// </summary>
/// <example>blight, defend, isolate, beast</example>
public class TokenClassToken : IToken, ITokenClass, IAppearInSpaceAbreviation {

	public TokenClassToken(string label, char initial, Img img) {
		Label = label;
		_summary = initial.ToString();
		Img = img;
	}

	/// <summary> Invisible token constructor </summary>
	public TokenClassToken( string label ) {
		Label = label;
		_summary = "";			// invisible, does not appear in summary list
		Img = Img.None;	// invisible, does not appear on board
	}

	#region Token

	ITokenClass IToken.Class => this;

	public Img Img { get; }

	public override string ToString() => _summary;

	readonly string _summary;
	string IOption.Text => Label;

	public string SpaceAbreviation => _summary;

	#endregion

	#region TokenGroup

	public string Label { get; }

	public bool HasTag( ITag tag ) => tag == this;

	#endregion

}