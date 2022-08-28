namespace SpiritIsland;

public class TerrainMapper {

	public virtual bool IsInPlay(Space space) => !space.Is(Terrain.Ocean);

	public virtual bool MatchesTerrain(Space space, params Terrain[] options) => space.IsOneOf(options);

	public virtual bool IsCoastal(Space space) => space.IsCoastal;

	public virtual bool IsInland( Space space ) => space.IsInland;

}
