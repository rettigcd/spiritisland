namespace SpiritIsland;

/// <summary>
/// A visible Entity appearing on a space.
/// </summary>
public interface IToken : ISpaceEntity, IOption {
	Img Img { get; }
	ITokenClass Class { get; } // Temporary Class to See if we can move off of ISpaceEntity
	bool HasTag(ITag tag);
	/// <summary> 1 or 2 letters that appear on the Token to help identify which variety it is.  (Dahan/Invader Health, Beast variants, Incarna powered up/down) </summary>
	string Badge { get; }
}

public static class ITokenExtensions {

	// avoid stupid (NewType) perenthsis need for casting
	public static HumanToken AsHuman(this IToken token) => (HumanToken)token;

}