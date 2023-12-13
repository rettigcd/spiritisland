namespace SpiritIsland;

public interface IToken : ISpaceEntity, IOption {
	Img Img { get; }
	ITokenClass Class { get; } // Temporary Class to See if we can move off of ISpaceEntity
	bool HasTag(ITag tag);
}

public static class ITokenExtensions {

	// avoid stupid (NewType) perenthsis need for casting
	public static HumanToken AsHuman(this IToken token) => (HumanToken)token;

}