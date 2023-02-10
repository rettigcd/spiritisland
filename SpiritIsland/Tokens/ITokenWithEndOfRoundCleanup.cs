namespace SpiritIsland;

public interface ITokenWithEndOfRoundCleanup : ISpaceEntity {
	void EndOfRoundCleanup(SpaceState spaceState);
}
