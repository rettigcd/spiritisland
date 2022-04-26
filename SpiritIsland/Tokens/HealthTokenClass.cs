namespace SpiritIsland;

public class HealthTokenClass : TokenClass {

	public HealthTokenClass( string label, int attack, TokenCategory category, int fearGeneratedWhenDestroyed ) {
		Label = label;
		Initial = label[0];
		Category = category;
		Attack = attack;

		this.FearGeneratedWhenDestroyed = fearGeneratedWhenDestroyed;

	}

	public char Initial { get; }

	public string Label { get; }

	public bool IsInvader => Category == TokenCategory.Invader;

	public TokenCategory Category { get; }

	/// <summary>
	/// Damage Inflicted during attack/defend
	/// </summary>
	public int Attack { get;}

	public int FearGeneratedWhenDestroyed { get; }

}