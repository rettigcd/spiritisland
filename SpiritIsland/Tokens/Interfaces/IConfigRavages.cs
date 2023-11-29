namespace SpiritIsland;

public interface IConfigRavages : ISpaceEntity {
	void Config( SpaceState space );
}

public interface IConfigRavagesAsync : ISpaceEntity {
	Task ConfigAsync( SpaceState space );
}
