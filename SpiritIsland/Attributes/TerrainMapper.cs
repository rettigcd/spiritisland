namespace SpiritIsland;

public class TerrainMapper {

	public virtual bool IsOneOf(Space space, params Terrain[] options) => space.IsOneOf(options);
	public virtual bool IsCoastal(Space space) => space.IsCoastal;

	public bool IsInPlay( Space space ) => !IsOneOf(space, Terrain.Ocean );

}
