namespace SpiritIsland;

public class HumanTokenClass(
	string _singular,
	string _plural,
	TokenCategory _categoryTag, 
	int _fearGeneratedWhenDestroyed, 
	Img _img, 
	int _expectedHealth
) 
	: ITokenClass
	, ITag
{
	public char Initial { get; } = _singular[0];

	/// <summary>
	/// The word describing 1 of these. ('Explorer','Town','City','Dahan')
	/// </summary>
	public string Label => _singular;

	public int FearGeneratedWhenDestroyed { get; } = _fearGeneratedWhenDestroyed;

	public Img Img { get; } = _img;

	/// <summary> 3 for cities, 2 for towns/dahan, 1 for explorers. </summary>
	/// <remarks> Helps UI display the health when different than expected </remarks>
	public int ExpectedHealthHint { get; } = _expectedHealth;

	public bool HasTag( ITag tag ) => tag == this || tag == _categoryTag;

	public string ToCountString( int count ) => (count == 1 ? $"1 {_singular}" : $"{count} {_plural}");

}