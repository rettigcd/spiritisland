namespace SpiritIsland;

/// <summary>
/// Provides 3 services
/// * Terrain Type
/// * Ocean/Coastal/Inland
/// * IsInPlay
/// </summary>
public class TerrainMapper {

	#region static

	public static TerrainMapper Current => ActionScope.Current.TerrainMapper;

	#endregion

	/// <summary> The space is Coastal or Inland.  aka Can-Hold-Tokens, aka NotOcean </summary>
	public virtual bool IsInPlay(Space space) => !space.SpaceSpec.Is(Terrain.Ocean);

	// Terrain
	public virtual bool MatchesTerrain( Space ss, params Terrain[] options ) => ss.SpaceSpec.IsOneOf( options );

	// Ocean / Coastal / Inland
	public virtual bool IsCoastal( Space space ) => space.SpaceSpec.IsCoastal;

#pragma warning disable CA1822 // Mark members as static
	public bool IsInland( Space space ) => !space.SpaceSpec.IsOcean && !space.SpaceSpec.IsCoastal;
#pragma warning restore CA1822 // Mark members as static

}
