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

	public virtual bool MatchesTerrain( SpaceState ss, params Terrain[] options ) => ss.Space.IsOneOf( options );



	public bool IsInPlay( SpaceState spaceState ) => IsInPlay( spaceState.Space );
	public virtual bool IsInPlay( Space space ) => !space.Is( Terrain.Ocean );


	public virtual bool IsCoastal( SpaceState spaceState ) => IsCoastal( spaceState.Space );
	public bool IsCoastal( Space space ) => space.IsCoastal;


	public bool IsInland( SpaceState spaceState ) => !spaceState.Space.IsOcean && !spaceState.Space.IsCoastal;

}
