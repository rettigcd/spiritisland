namespace SpiritIsland;

public interface ITokenWithEndOfRoundCleanup : IToken {
	void EndOfRoundCleanup(SpaceState spaceState);
}
