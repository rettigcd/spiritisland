namespace SpiritIsland;

/// <summary>
/// Token that implements its own IEntity.Class
/// </summary>
/// <example>blight, defend, isolate, beast</example>
public class TokenClassToken(string label, char spaceAbrev, Img img, string? badge = null) : IToken, ITokenClass {

	#region IOption interface

	string IOption.Text => label;

	#endregion IOption interface

	#region Token

	public Img Img => img;
	ITokenClass IToken.Class => this;
	string IToken.Badge => _badge;
	string IToken.SpaceAbreviation => _spaceAbrev;
	public bool HasTag(ITag tag) => tag == this || tag == BonusTag;

	public ITag? BonusTag = null; // Hook for treating some things as other things

	readonly string _badge = badge ?? string.Empty;

	#endregion Token

	#region ITokenClass

	public string Label => label;

	#endregion ITokenClass

	public override string ToString() => _spaceAbrev;

	readonly string _spaceAbrev = spaceAbrev.ToString();

}
