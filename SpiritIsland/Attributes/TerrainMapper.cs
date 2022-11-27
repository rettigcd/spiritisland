namespace SpiritIsland;

public class TerrainMapper {

	public virtual bool IsInPlay(Space space) => !space.Is(Terrain.Ocean);

	public virtual bool MatchesTerrain(SpaceState ss, params Terrain[] options) => ss.Space.IsOneOf(options);

	public virtual bool IsCoastal(Space space) => space.IsCoastal;

	public virtual bool IsInland( Space space ) => !space.IsOcean && !space.IsCoastal;

	public bool IsInland( SpaceState spaceState ) => IsInland(spaceState.Space);
	public bool IsInPlay( SpaceState spaceState ) => IsInPlay( spaceState.Space );
	public bool IsCoastal( SpaceState spaceState ) => IsCoastal( spaceState.Space );

}
