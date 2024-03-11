namespace SpiritIsland;

public interface ISpaceEntityWithEndOfRoundCleanup : ISpaceEntity {
	void EndOfRoundCleanup(Space space);
}

public interface IEndWhenTimePasses : ISpaceEntity { }
