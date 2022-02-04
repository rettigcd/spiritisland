namespace SpiritIsland;

public interface TokenClass {

	Token Default { get; }

	char Initial { get; }

	string Label { get; }

	void ExtendHealthRange( int newMaxHealth );

	Token this[int i] { get; }

	TokenCategory Category { get; }
}