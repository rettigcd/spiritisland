namespace SpiritIsland;

public class HealthTokenClass : TokenClass {

	public HealthTokenClass( string label, int attack, TokenCategory category, int fearGeneratedWhenDestroyed, Img img, int expectedHealth ) {
		Label = label;
		Initial = label[0];
		Category = category;
		Attack = attack;
		FearGeneratedWhenDestroyed = fearGeneratedWhenDestroyed;
		Img = img;
		ExpectedHealthHint = expectedHealth;
	}

	public char Initial { get; }

	public string Label { get; }

	public bool IsInvader => Category == TokenCategory.Invader;

	public TokenCategory Category { get; }

	/// <summary> Damage Inflicted during attack/defend </summary>
	public int Attack { get;}

	public int FearGeneratedWhenDestroyed { get; }

	public Img Img { get; }

	/// <summary> 3 for cities, 2 for towns/dahan, 1 for explorers. </summary>
	/// <remarks> Helps UI display the health when different than expected </remarks>
	public int ExpectedHealthHint { get; }

}