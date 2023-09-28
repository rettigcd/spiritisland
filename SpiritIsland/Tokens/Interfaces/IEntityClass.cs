namespace SpiritIsland;

/// <summary> A class-of token, not a token itself. </summary>
public interface IEntityClass {

	string Label { get; }

	TokenCategory Category { get; }
}