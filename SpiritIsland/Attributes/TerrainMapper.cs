namespace SpiritIsland;

public class TerrainMapper {

	public virtual bool MatchesTerrain(Space space, params Terrain[] options) => space.IsOneOf(options);

	public virtual bool IsCoastal(Space space) => space.IsCoastal;

}
