namespace SpiritIsland;

/// <summary>
/// Provides 2 services:
///		1) Nominal range from source
///		2) Binding to a Spirit returns a Predicate for matching spaces.
/// </summary>
public class TargetCriteria {

	#region constructor

	/// <summary> For no-filter criteria </summary>
	public TargetCriteria( int range ) {
		Range = range;
		_filters = Array.Empty<string>();
		_self = null; // Don't need to bind spirit since no filters test for spirit.
	}

	/// <summary> For early binding Spirit dependent criteria </summary>
	public TargetCriteria( int range, Spirit self, params string[] filters ) {
		Range = range;
		_self = self;
		_filters = filters ?? throw new ArgumentNullException( nameof( filters ) );
	}

	#endregion

	public int Range { get; }

	public bool Matches( SpaceState state ){

		// since we are doing a MatchAny (OR), we need at least 1 criteria or it won't match anything
		// (if we were to do a MatchAll (AND), then we wouldn't need any criteria)
		if(_filters.Length == 0) return true;

		SpaceStateWithPresence allStateData = new SpaceStateWithPresence( state, _self, TerrainMapper );

		if(_filters.Length == 1) return Matches(_filters[0], allStateData );

		// Get the filters 
		return _filters.Any( f => Matches(f,allStateData) );
	}

	// Virtual so OfferPassageBetweenWorlds can do multiple criteria
	public virtual TargetCriteria ExtendRange( int extension ) => new TargetCriteria( Range + extension, _self, _filters );

	#region private
	readonly string[] _filters;
	TerrainMapper TerrainMapper => _terrainMapper ??= ActionScope.Current.TerrainMapper;
	TerrainMapper _terrainMapper;
	readonly Spirit _self;
	#endregion

	#region Bind all state together into a single object

	class SpaceStateWithPresence {

		#region constructor
		public SpaceStateWithPresence( SpaceState spaceState, Spirit focusSpirit, TerrainMapper tm ) {
			Tokens = spaceState;
			_focusSpirit = focusSpirit;
			_terrainMapper = tm;
		}
		#endregion

		public SpaceState Tokens { get; }

		// Bound Spirit
		public bool IsPresent => _focusSpirit.Presence.IsOn(Tokens);
		readonly Spirit _focusSpirit;

		// Bound TerrainMapper
		public bool MatchesTerrain( Terrain terrain ) => _terrainMapper.MatchesTerrain( Tokens, terrain );
		public bool IsCoastal => _terrainMapper.IsCoastal( Tokens );
		public bool IsInland => _terrainMapper.IsInland( Tokens );
		readonly TerrainMapper _terrainMapper;

	}
	#endregion

	#region static Filter Map

	static bool Matches( string filterEnum, SpaceStateWithPresence state ) {
		return _lookup.ContainsKey( filterEnum )
			? _lookup[filterEnum]( state )
			: throw new ArgumentException( "Unexpected filter:" + filterEnum, nameof( filterEnum ) );
	}

	static readonly Dictionary<string,Func<SpaceStateWithPresence, bool>> _lookup = new Dictionary<string, Func<SpaceStateWithPresence, bool>> {
		[Target.Any               ] = (_)   => true,

		// Terrain
		[Target.Jungle            ] = (ctx) => ctx.MatchesTerrain( Terrain.Jungle ),
		[Target.Mountain          ] = (ctx) => ctx.MatchesTerrain( Terrain.Mountain ),
		[Target.Sand              ] = (ctx) => ctx.MatchesTerrain( Terrain.Sand ),
		[Target.Wetland           ] = (ctx) => ctx.MatchesTerrain( Terrain.Wetland ),
		[Target.Coastal           ] = (ctx) => ctx.IsCoastal,
		[Target.NotWetland        ] = (ctx) => !ctx.MatchesTerrain( Terrain.Wetland ),
		[Target.NotOcean          ] = (ctx) => !ctx.Tokens.Space.Is( Terrain.Ocean ), // even when ocean is is play, not allowed 
		[Target.Inland            ] = (ctx) => ctx.IsInland,

		// Dahan
		[Target.Dahan             ] = (ctx) => ctx.Tokens.Dahan.Any,

		// Invaders
		[Target.Invaders          ] = (ctx) => ctx.Tokens.HasInvaders(),
		[Target.City              ] = (ctx) => ctx.Tokens.Has(Human.City),
		[Target.NoInvader         ] = (ctx) => !ctx.Tokens.HasInvaders(),

		[Target.Disease           ] = (ctx) => ctx.Tokens.Disease.Any,
		[Target.Beast             ] = (ctx) => ctx.Tokens.Beasts.Any,
		[Target.Wilds             ] = (ctx) => ctx.Tokens.Wilds.Any,

		[Target.Blight            ] = (ctx) => ctx.Tokens.Blight.Any,
		[Target.NoBlight          ] = (ctx) => !ctx.Tokens.Blight.Any,

		// Presence
		[Target.Presence          ] = (ctx) => ctx.IsPresent,

		// Special
		[Target.TwoBeasts         ] = (ctx) => 2<=ctx.Tokens.Beasts.Count,
		[Target.BlightAndInvaders ] = (ctx) => ctx.Tokens.Blight.Any && ctx.Tokens.HasInvaders(),
		[Target.TwoBeastPlusInvaders]=(ctx) => 2 <= ctx.Tokens.Beasts.Count && ctx.Tokens.HasInvaders(),

	};
	#endregion

}
