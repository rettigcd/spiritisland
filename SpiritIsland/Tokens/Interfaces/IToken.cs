namespace SpiritIsland;

public interface IToken : ISpaceEntity, IOption {
	Img Img { get; }
}

public static class ITokenExtensions {

	// avoid stupid (NewType) perenthsis need for casting
	public static HumanToken AsHuman(this IToken token) => (HumanToken)token;
}