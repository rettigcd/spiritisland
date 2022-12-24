namespace SpiritIsland;

public class HealthTokenClass : TokenClass {

	public HealthTokenClass( string label, int attack, TokenCategory category, int fearGeneratedWhenDestroyed, Img img, int expectedHealth, TokenVariant variant=default ) {
		Label = label;
		Attack = attack;
		Category = category;
		FearGeneratedWhenDestroyed = fearGeneratedWhenDestroyed;
		Img = img;
		ExpectedHealthHint = expectedHealth;
		Variant = variant;

		Initial = label[0];
	}

	public char Initial { get; }

	public string Label { get; }

	public bool IsInvader => Category == TokenCategory.Invader;

	public TokenCategory Category { get; }

	/// <summary> Damage Inflicted during attack/defend </summary>
	public int Attack { get; set; } // changed by sweeden

	public int FearGeneratedWhenDestroyed { get; }

	public Img Img { get; }

	public TokenVariant Variant { get; }

	/// <summary> 3 for cities, 2 for towns/dahan, 1 for explorers. </summary>
	/// <remarks> Helps UI display the health when different than expected </remarks>
	public int ExpectedHealthHint { get; }

}

public enum TokenVariant {
	Default,
	Dreaming,
	Frozen
}