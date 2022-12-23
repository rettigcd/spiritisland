namespace SpiritIsland;

/// <summary>
/// Provides 3 services
/// * Terrain Type
/// * Ocean/Coastal/Inland
/// * IsInPlay
/// </summary>
public class TerrainMapper {

	public TargetCriteria Specify( int range, params string[] filters)
		=> new TargetCriteria(this, range, filters);

	// Terrain
	public virtual bool MatchesTerrain( SpaceState ss, params Terrain[] options ) => ss.Space.IsOneOf( options );

	// InPlay
	public bool IsInPlay( SpaceState spaceState ) => IsInPlay( spaceState.Space );
	public virtual bool IsInPlay( Space space ) => !space.Is( Terrain.Ocean );

	// Ocean / Coastal / Inland
	public virtual bool IsCoastal( SpaceState spaceState ) => spaceState.Space.IsCoastal;
	public bool IsInland( SpaceState spaceState ) => !spaceState.Space.IsOcean && !spaceState.Space.IsCoastal;

}
