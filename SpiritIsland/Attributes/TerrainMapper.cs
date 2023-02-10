using SpiritIsland.Select;

namespace SpiritIsland;

/// <summary>
/// Provides 3 services
/// * Terrain Type
/// * Ocean/Coastal/Inland
/// * IsInPlay
/// </summary>
public class TerrainMapper {

	// Terrain
	public virtual bool MatchesTerrain( SpaceState ss, params Terrain[] options ) => ss.Space.IsOneOf( options );

	// InPlay
	/// <summary> The space is Coastal or Inland.  aka Can-Hold-Tokens, aka NotOcean </summary>
	public bool IsInPlay( SpaceState spaceState ) => !spaceState.Space.Is( Terrain.Ocean );

	public virtual bool IsInPlay( Space space ) => !space.Is( Terrain.Ocean );

	// Ocean / Coastal / Inland
	public virtual bool IsCoastal( SpaceState spaceState ) => spaceState.Space.IsCoastal;

#pragma warning disable CA1822 // Mark members as static
	public bool IsInland( SpaceState spaceState ) => !spaceState.Space.IsOcean && !spaceState.Space.IsCoastal;
#pragma warning restore CA1822 // Mark members as static

}
