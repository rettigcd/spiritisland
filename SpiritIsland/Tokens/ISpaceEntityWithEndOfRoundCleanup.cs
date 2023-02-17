namespace SpiritIsland;

public interface ISpaceEntityWithEndOfRoundCleanup : ISpaceEntity {
	void EndOfRoundCleanup(SpaceState tokens);
}

public interface IEndWhenTimePasses : ISpaceEntity { }
