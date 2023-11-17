namespace SpiritIsland;

public class HumanTokenClass : ITokenClass, ITag {

	public HumanTokenClass( string label, TokenCategory categoryTag, int fearGeneratedWhenDestroyed, Img img, int expectedHealth, TokenVariant variant=default ) {
		Label = label;
		_catTag = categoryTag;
		FearGeneratedWhenDestroyed = fearGeneratedWhenDestroyed;
		Img = img;
		ExpectedHealthHint = expectedHealth;
		Variant = variant;

		Initial = label[0];
	}

	public char Initial { get; }

	public string Label { get; }

	public int FearGeneratedWhenDestroyed { get; }

	public Img Img { get; }

	public TokenVariant Variant { get; }

	/// <summary> 3 for cities, 2 for towns/dahan, 1 for explorers. </summary>
	/// <remarks> Helps UI display the health when different than expected </remarks>
	public int ExpectedHealthHint { get; }

	public bool HasTag( ITag tag ) => tag == this || tag == _catTag;
	readonly TokenCategory _catTag;
}

public enum TokenVariant {
	Default,
	Dreaming,
	Frozen
}