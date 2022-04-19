namespace SpiritIsland;

public class HealthTokenClass : TokenClass {

	public HealthTokenClass( string label, int fullHealth, TokenCategory category, int fearGeneratedWhenDestroyed ) {
		Label = label;
		Initial = label[0];
		Category = category;

		this.FearGeneratedWhenDestroyed = fearGeneratedWhenDestroyed;

	}

	public char Initial { get; }

	public string Label { get; }

	public bool IsInvader => Category == TokenCategory.Invader;

	public TokenCategory Category { get; }

	public int FearGeneratedWhenDestroyed { get; }

}