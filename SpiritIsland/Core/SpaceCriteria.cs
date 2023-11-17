namespace SpiritIsland;

/// <summary>
/// Used for filtering Target: Sources & Destinations
/// </summary>
public class SpaceCriteria {

	/// <summary> Create a space-criteria that accepts everything. </summary>
	public SpaceCriteria() {
		_filters = Array.Empty<string>();
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
				return FocusSpirit(nameof(HasIncarna)).Presence is IHaveIncarna carny
					&& carny.Incarna.Space?.Space == Tokens.Space;
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
		if(_lookup.ContainsKey( filterEnum ))
			return _lookup[filterEnum]( state );

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
		[Target.Any] = ( _ ) => true,

		// Terrain
		[Target.Jungle] = ( ctx ) => ctx.MatchesTerrain( Terrain.Jungle ),
		[Target.Mountain] = ( ctx ) => ctx.MatchesTerrain( Terrain.Mountain ),
		[Target.Sand] = ( ctx ) => ctx.MatchesTerrain( Terrain.Sand ),
		[Target.Wetland] = ( ctx ) => ctx.MatchesTerrain( Terrain.Wetland ),
		[Target.Ocean] = ( ctx ) => ctx.Tokens.Space.Is( Terrain.Ocean ),
		[Target.Coastal] = ( ctx ) => ctx.IsCoastal,
		[Target.NotWetland] = ( ctx ) => !ctx.MatchesTerrain( Terrain.Wetland ),
		[Target.NotOcean] = ( ctx ) => !ctx.Tokens.Space.Is( Terrain.Ocean ), // even when ocean is is play, not allowed 
		[Target.Inland] = ( ctx ) => ctx.IsInland,

		// Dahan
		[Target.Dahan] = ( ctx ) => ctx.Tokens.Dahan.Any,

		// Invaders
		[Target.Invaders] = ( ctx ) => ctx.Tokens.HasInvaders(),
		[Target.City] = ( ctx ) => ctx.Tokens.Has( Human.City ),
		[Target.Town] = ( ctx ) => ctx.Tokens.Has( Human.Town ),
		[Target.NoInvader] = ( ctx ) => !ctx.Tokens.HasInvaders(),

		// Tokens
		[Target.Disease] = ( ctx ) => ctx.Tokens.Disease.Any,
		[Target.Beast] = ( ctx ) => ctx.Tokens.Beasts.Any,
		[Target.Wilds] = ( ctx ) => ctx.Tokens.Wilds.Any,
		[Target.Quake] = ( ctx ) => ctx.Tokens.Has(Token.Quake),

		[Target.Blight] = ( ctx ) => ctx.Tokens.Blight.Any,
		[Target.NoBlight] = ( ctx ) => !ctx.Tokens.Blight.Any,

		// Presence
		[Target.Presence] = ( ctx ) => ctx.IsPresent,
		[Target.Incarna] = ( ctx ) => ctx.HasIncarna,
		[Target.EndlessDark] = ( ctx ) => ctx.Tokens.Space.Text == "EndlessDark",

		// Special
		[Target.TwoBeasts] = ( ctx ) => 2 <= ctx.Tokens.Beasts.Count,
		[Target.BlightAndInvaders] = ( ctx ) => ctx.Tokens.Blight.Any && ctx.Tokens.HasInvaders(),
		[Target.TwoBeastPlusInvaders] = ( ctx ) => 2 <= ctx.Tokens.Beasts.Count && ctx.Tokens.HasInvaders(),

	};
	#endregion

}
