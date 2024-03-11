namespace SpiritIsland;

public interface IConfigRavages : ISpaceEntity {
	void Config( Space space );
}

public interface IConfigRavagesAsync : ISpaceEntity {
	Task ConfigAsync( Space space );
}
