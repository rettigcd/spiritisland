namespace SpiritIsland;

public class TokenVariety(TokenClassToken original, string badge) : IToken, IAppearInSpaceAbreviation {
	#region IToken
	Img IToken.Img => _asToken.Img;
	ITokenClass IToken.Class => original;
	string IToken.Badge => badge;
	bool IToken.HasTag(ITag tag) => _asToken.HasTag(tag);
	#endregion IToken
	string IOption.Text => $"{_asClass.Label}-{badge}";

	IToken _asToken => original;
	ITokenClass _asClass => original;
	IAppearInSpaceAbreviation _asSpaceVisible => original;

	#region IAppearInSpaceAbreviation
	string IAppearInSpaceAbreviation.SpaceAbreviation => $"{_asSpaceVisible.SpaceAbreviation}-{badge}";
	#endregion IAppearInSpaceAbreviation
}