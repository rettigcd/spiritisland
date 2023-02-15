namespace SpiritIsland;

public interface ISpaceEntityWithEndOfRoundCleanup : ISpaceEntity {
	void EndOfRoundCleanup(SpaceState spaceState);
}
