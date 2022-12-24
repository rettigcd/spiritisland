namespace SpiritIsland;

/// <summary>
/// Provides 2 services:
///		1) Nominal range from source
///		2) Binding to a Spirit returns a Predicate for matching spaces.
/// </summary>
public class TargetCriteria {

	public int Range { get; }
	readonly string[] _filters;
	readonly TerrainMapper _terrainMapper;
	readonly Spirit _self;

	/// <summary>
	/// For non-spirit dependent criteria
	/// </summary>
	public TargetCriteria( TerrainMapper terrainMapper, int range, params string[] filters ) {
		_terrainMapper = terrainMapper;
		Range = range;
		_filters = filters ?? throw new ArgumentNullException( nameof( filters ) );
	}

	/// <summary> For early binding Spirit dependent criteria </summary>
	public TargetCriteria( TerrainMapper terrainMapper, int range, Spirit self, params string[] filters ) {
		_terrainMapper = terrainMapper;
		Range = range;
		_self = self;
		_filters = filters ?? throw new ArgumentNullException( nameof( filters ) );
	}

	// Virtual so OfferPassageBetweenWorlds can do multiple criteria
	public virtual TargetCriteria ExtendRange( int extension ) => new TargetCriteria( _terrainMapper, Range + extension, _filters );

	public bool Matches(SpaceState x){	// ??? Could we early-bind to the spirit the same way we early-bind to the Terrain mapper?
		// !!! Since all parameters were early-bound, we can merge the Space Filter Map into this class.

		// since we are doing a MatchAny (OR), we need at least 1 criteria or it won't match anything
		// if we were to do a MatchAll (AND), then we wouldn't need any criteria
		return _filters.Length == 0
			? _terrainMapper.IsInPlay(x)	// special case, no criteria => return true
			: SpaceFilterMap.MatchAny( _self, _terrainMapper, _filters )(x); // otherwise batch any of the filters
	}
}
