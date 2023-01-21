
namespace SpiritIsland;

/// <summary> Used by Invader Cards </summary>
public interface InvaderCardSpaceFilter {
	bool Matches( Space space );
	string Text { get; }
}
