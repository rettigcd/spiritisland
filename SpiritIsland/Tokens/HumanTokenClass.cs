namespace SpiritIsland;

public class HumanTokenClass(
	string _label, 
	TokenCategory _categoryTag, 
	int _fearGeneratedWhenDestroyed, 
	Img _img, 
	int _expectedHealth, 
	TokenVariant _variant = default
) 
	: ITokenClass
	, ITag
{
	public char Initial { get; } = _label[0];

	public string Label { get; } = _label;

	public int FearGeneratedWhenDestroyed { get; } = _fearGeneratedWhenDestroyed;

	public Img Img { get; } = _img;

	public TokenVariant Variant { get; } = _variant;

	/// <summary> 3 for cities, 2 for towns/dahan, 1 for explorers. </summary>
	/// <remarks> Helps UI display the health when different than expected </remarks>
	public int ExpectedHealthHint { get; } = _expectedHealth;

	public bool HasTag( ITag tag ) => tag == this || tag == _categoryTag;
}

public enum TokenVariant {
	Default,
	Dreaming,
	Frozen
}