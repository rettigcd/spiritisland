namespace SpiritIsland;

/// <summary>
/// Used for filtering Target: Sources & Destinations
/// </summary>
public class SpaceCriteria {

	/// <summary> Create a space-criteria that accepts everything. </summary>
	public SpaceCriteria() {
		_filters = [];
		_self = null; // Don't need to bind spirit since no filters test for spirit.
	}
	public SpaceCriteria(Spirit self, params string[] filters) {
		_self = self;
		_filters = filters ?? throw new ArgumentNullException( nameof( filters ) );
	}

	public bool Matches( SpaceState state ) {

		// since we are doing a MatchAny (OR), we need at least 1 criteria or it won't match anything
		// (if we were to do a MatchAll (AND), then we wouldn't need any criteria)
		if(_filters.Length == 0) return true;

		SpaceStateWithPresence allStateData = new SpaceStateWithPresence( state, _self, TerrainMapper );

		if(_filters.Length == 1) return Matches( _filters[0], allStateData );

		// Get the filters 
		return _filters.Any( f => Matches( f, allStateData ) );
	}

	#region private
	readonly protected Spirit _self;
	readonly protected string[] _filters; // Any one of these filters can match.
	TerrainMapper TerrainMapper => _terrainMapper ??= ActionScope.Current.TerrainMapper;
	TerrainMapper _terrainMapper;
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
		public bool IsPresent => FocusSpirit( nameof( IsPresent ) ).Presence.IsOn( Tokens );
		public bool HasIncarna {
			get {
				var incarna = FocusSpirit(nameof(HasIncarna)).Incarna;
				return Tokens.Space == incarna.Space.Space;
			}
		}
		Spirit FocusSpirit(string opName) => _focusSpirit ?? throw new InvalidOperationException($"Spirit not available for testing {opName}.");
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
		if(_lookup.TryGetValue( filterEnum, out Func<SpaceStateWithPresence, bool> value ))
			return value( state );

		if(filterEnum.Contains( '+' )) {
			var filter = new AllFilters( filterEnum );
			_lookup[filterEnum] = filter.Matches;
			return filter.Matches( state );
		}

		throw new ArgumentException( "Unexpected filter:" + filterEnum, nameof( filterEnum ) );
	}

	class AllFilters {
		public AllFilters( string combinedFilter ) {
			_parts = combinedFilter.Split( '+' ).Select( s => _lookup[s] ).ToArray();
		}
		public bool Matches( SpaceStateWithPresence x ) => _parts.All( p => p( x ) );
		readonly Func<SpaceStateWithPresence, bool>[] _parts;
	}

	static readonly Dictionary<string, Func<SpaceStateWithPresence, bool>> _lookup = new Dictionary<string, Func<SpaceStateWithPresence, bool>> {
		[Filter.Any] = ( _ ) => true,

		// Terrain
		[Filter.Jungle]     = ( ctx ) => ctx.MatchesTerrain( Terrain.Jungle ),
		[Filter.Mountain]   = ( ctx ) => ctx.MatchesTerrain( Terrain.Mountain ),
		[Filter.Sands]      = ( ctx ) => ctx.MatchesTerrain( Terrain.Sands ),
		[Filter.Wetland]    = ( ctx ) => ctx.MatchesTerrain( Terrain.Wetland ),
		[Filter.Ocean]      = ( ctx ) => ctx.Tokens.Space.Is( Terrain.Ocean ),
		[Filter.Coastal]    = ( ctx ) => ctx.IsCoastal,
		[Filter.NotWetland] = ( ctx ) => !ctx.MatchesTerrain( Terrain.Wetland ),
		[Filter.NotOcean]   = ( ctx ) => !ctx.Tokens.Space.Is( Terrain.Ocean ), // even when ocean is is play, not allowed 
		[Filter.Inland]     = ( ctx ) => ctx.IsInland,

		// Dahan
		[Filter.Dahan] = ( ctx ) => ctx.Tokens.Dahan.Any,

		// Invaders
		[Filter.Town]      = ( ctx ) => ctx.Tokens.Has( Human.Town ),
		[Filter.City]      = ( ctx ) => ctx.Tokens.Has( Human.City ),
		[Filter.Invaders]  = ( ctx ) => ctx.Tokens.HasInvaders(),
		[Filter.NoInvader] = ( ctx ) => !ctx.Tokens.HasInvaders(),

		// Tokens
		[Filter.Disease] = ( ctx ) => ctx.Tokens.Disease.Any,
		[Filter.Beast] = ( ctx ) => ctx.Tokens.Beasts.Any,
		[Filter.Wilds] = ( ctx ) => ctx.Tokens.Wilds.Any,
		[Filter.Quake] = ( ctx ) => ctx.Tokens.Has(Token.Quake),

		[Filter.Blight] = ( ctx ) => ctx.Tokens.Blight.Any,
		[Filter.Strife] = ( ctx ) => ctx.Tokens.HasStrife,
		[Filter.NoBlight] = ( ctx ) => !ctx.Tokens.Blight.Any,

		// Presence
		[Filter.Presence] = ( ctx ) => ctx.IsPresent,
		[Filter.Incarna] = ( ctx ) => ctx.HasIncarna,
		[Filter.EndlessDark] = ( ctx ) => ctx.Tokens.Space.Text == "EndlessDark",

		// Special
		[Filter.TwoDahan]  = ( ctx ) => 2 <= ctx.Tokens.Dahan.CountAll,
		[Filter.TwoBeasts] = ( ctx ) => 2 <= ctx.Tokens.Beasts.Count,
		[Filter.CoastalCity] = (ctx) => ctx.IsCoastal && ctx.Tokens.Has( Human.City ),
	};
	#endregion

}
