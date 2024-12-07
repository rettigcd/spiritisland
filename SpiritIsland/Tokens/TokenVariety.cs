namespace SpiritIsland;

public class TokenVariety(TokenClassToken original, string badge) : IToken, IAppearInSpaceAbreviation {
	#region IToken
	Img IToken.Img => original.Img;
	ITokenClass IToken.Class => original;
	string IToken.Badge => badge;
	bool IToken.HasTag(ITag tag) => original.HasTag(tag);
	#endregion IToken
	string IOption.Text => $"{original.Label}-{badge}";
	#region IAppearInSpaceAbreviation
	string IAppearInSpaceAbreviation.SpaceAbreviation => $"{original.SpaceAbreviation}-{badge}";
	#endregion IAppearInSpaceAbreviation
}