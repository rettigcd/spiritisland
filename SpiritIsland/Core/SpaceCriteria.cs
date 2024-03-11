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

	public bool Matches( Space state ) {

		// since we are doing a MatchAny (OR), we need at least 1 criteria or it won't match anything
		// (if we were to do a MatchAll (AND), then we wouldn't need any criteria)
		if(_filters.Length == 0) return true;

		SpaceWithPresence allStateData = new SpaceWithPresence( state, _self, TerrainMapper );

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

	class SpaceWithPresence( Space space, Spirit focusSpirit, TerrainMapper tm ) {

		public Space Space { get; } = space;

		// Bound Spirit
		public bool IsPresent => FocusSpirit( nameof( IsPresent ) ).Presence.IsOn( Space );
		public bool HasIncarna {
			get {
				var incarna = FocusSpirit(nameof(HasIncarna)).Incarna;
				return Space.SpaceSpec == incarna.Space.SpaceSpec;
			}
		}
		Spirit FocusSpirit(string opName) => focusSpirit ?? throw new InvalidOperationException($"Spirit not available for testing {opName}.");

		// Bound TerrainMapper
		public bool MatchesTerrain( Terrain terrain ) => tm.MatchesTerrain( Space, terrain );
		public bool IsCoastal => tm.IsCoastal( Space );
		public bool IsInland => tm.IsInland( Space );
	}
	#endregion

	#region static Filter Map

	static bool Matches( string filterEnum, SpaceWithPresence state ) {
		if(_lookup.TryGetValue( filterEnum, out Func<SpaceWithPresence, bool> value ))
			return value( state );

		if(filterEnum.Contains( '+' )) {
			var filter = new AllFilters( filterEnum );
			_lookup[filterEnum] = filter.Matches;
			return filter.Matches( state );
		}

		throw new ArgumentException( "Unexpected filter:" + filterEnum, nameof( filterEnum ) );
	}

	class AllFilters( string combinedFilter ) {
		public bool Matches( SpaceWithPresence x ) => _parts.All( p => p( x ) );
		readonly Func<SpaceWithPresence, bool>[] _parts = combinedFilter.Split( '+' ).Select( s => _lookup[s] ).ToArray();
	}

	static readonly Dictionary<string, Func<SpaceWithPresence, bool>> _lookup = new Dictionary<string, Func<SpaceWithPresence, bool>> {
		[Filter.Any] = ( _ ) => true,

		// Terrain
		[Filter.Jungle]     = ( ctx ) => ctx.MatchesTerrain( Terrain.Jungle ),
		[Filter.Mountain]   = ( ctx ) => ctx.MatchesTerrain( Terrain.Mountain ),
		[Filter.Sands]      = ( ctx ) => ctx.MatchesTerrain( Terrain.Sands ),
		[Filter.Wetland]    = ( ctx ) => ctx.MatchesTerrain( Terrain.Wetland ),
		[Filter.Ocean]      = ( ctx ) => ctx.Space.SpaceSpec.Is( Terrain.Ocean ),
		[Filter.Coastal]    = ( ctx ) => ctx.IsCoastal,
		[Filter.NotWetland] = ( ctx ) => !ctx.MatchesTerrain( Terrain.Wetland ),
		[Filter.NotOcean]   = ( ctx ) => !ctx.Space.SpaceSpec.Is( Terrain.Ocean ), // even when ocean is is play, not allowed 
		[Filter.Inland]     = ( ctx ) => ctx.IsInland,

		// Dahan
		[Filter.Dahan] = ( ctx ) => ctx.Space.Dahan.Any,

		// Invaders
		[Filter.Town]      = ( ctx ) => ctx.Space.Has( Human.Town ),
		[Filter.City]      = ( ctx ) => ctx.Space.Has( Human.City ),
		[Filter.Invaders]  = ( ctx ) => ctx.Space.HasInvaders(),
		[Filter.NoInvader] = ( ctx ) => !ctx.Space.HasInvaders(),

		// Tokens
		[Filter.Disease] = ( ctx ) => ctx.Space.Disease.Any,
		[Filter.Beast] = ( ctx ) => ctx.Space.Beasts.Any,
		[Filter.Wilds] = ( ctx ) => ctx.Space.Wilds.Any,
		[Filter.Quake] = ( ctx ) => ctx.Space.Has(Token.Quake),

		[Filter.Blight] = ( ctx ) => ctx.Space.Blight.Any,
		[Filter.Strife] = ( ctx ) => ctx.Space.HasStrife,
		[Filter.NoBlight] = ( ctx ) => !ctx.Space.Blight.Any,

		// Presence
		[Filter.Presence] = ( ctx ) => ctx.IsPresent,
		[Filter.Incarna] = ( ctx ) => ctx.HasIncarna,
		[Filter.EndlessDark] = ( ctx ) => ctx.Space.SpaceSpec.Label == "EndlessDark",

		// Special
		[Filter.TwoDahan]  = ( ctx ) => 2 <= ctx.Space.Dahan.CountAll,
		[Filter.TwoBeasts] = ( ctx ) => 2 <= ctx.Space.Beasts.Count,
		[Filter.CoastalCity] = (ctx) => ctx.IsCoastal && ctx.Space.Has( Human.City ),
	};
	#endregion

}
