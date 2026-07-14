namespace SpiritIsland;

public class TokenVariety(TokenClassToken original, string badge) : IToken {
	#region IToken
	Img IToken.Img => _asToken.Img;
	ITokenClass IToken.Class => original;
	string IToken.Badge => badge;
	string IToken.SpaceAbreviation => $"{_asToken.SpaceAbreviation}-{badge}";
	bool IToken.HasTag(ITag tag) => _asToken.HasTag(tag);
	#endregion IToken
	string IOption.Text => $"{_asClass.Label}-{badge}";

	IToken _asToken => original;
	ITokenClass _asClass => original;
}