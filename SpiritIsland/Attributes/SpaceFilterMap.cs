namespace SpiritIsland;

public class SpaceFilterMap {

	static readonly Dictionary<string,Func<SpaceStateWithPresence, bool>> lookup = new Dictionary<string, Func<SpaceStateWithPresence, bool>> {
		[Target.Any               ] = (_)   => true,

		// Terrain
		[Target.Jungle            ] = (ctx) => ctx.MatchesTerrain( Terrain.Jungle ),
		[Target.Mountain          ] = (ctx) => ctx.MatchesTerrain( Terrain.Mountain ),
		[Target.Sand              ] = (ctx) => ctx.MatchesTerrain( Terrain.Sand ),
		[Target.Wetland           ] = (ctx) => ctx.MatchesTerrain( Terrain.Wetland ),
		[Target.Coastal           ] = (ctx) => ctx.IsCoastal,
		[Target.NotWetland        ] = (ctx) => !ctx.MatchesTerrain( Terrain.Wetland ),
		[Target.NotOcean          ] = (ctx) => !ctx.Tokens.Space.Is( Terrain.Ocean ), // even when ocean is is play, not allowed 
		[Target.Inland            ] = (ctx) => ctx.TerrainMapper.IsInland( ctx.Tokens ),

		// Dahan
		[Target.Dahan             ] = (ctx) => ctx.Tokens.Dahan.Any,

		// Invaders
		[Target.Invaders          ] = (ctx) => ctx.Tokens.HasInvaders(),
		[Target.ExplorerOrTown    ] = (ctx) => ctx.Tokens.HasAny( Invader.Explorer_Town ),  // Wash Away helper
		[Target.TownOrCity        ] = (ctx) => ctx.Tokens.HasAny( Invader.Town_City ),  // Study the Invaders' Fears
		[Target.City              ] = (ctx) => ctx.Tokens.Has(Invader.City),
		[Target.NoInvader         ] = (ctx) => !ctx.Tokens.HasInvaders(),

		[Target.Disease           ] = (ctx) => ctx.Tokens.Disease.Any,
		[Target.Beast             ] = (ctx) => ctx.Tokens.Beasts.Any,
		[Target.Wilds             ] = (ctx) => ctx.Tokens.Wilds.Any,

		[Target.Blight            ] = (ctx) => ctx.Tokens.Blight.Any,
		[Target.NoBlight          ] = (ctx) => !ctx.Tokens.Blight.Any,

		// Register new filters needed for Branch and Claw
		[Target.Presence          ] = (ctx) => ctx.IsPresent,

		// Jagged Earth
		[Target.TwoBeasts         ] = (ctx) => ctx.Tokens.Beasts.Count>=2,


	};

	// !!! Problems with reducing input to just Space: Focus Presence
	// We want to reduce this to just SpaceState/Tokens because we don't want to generate an Action, simply for evaluating which we can Target.

	/// <remarks>Can NOT call this from Cards Static constructor because it is not invoked until AFTER the attribute is evaluated</remarks>
	public static void Register(string targetEnum,Func<SpaceStateWithPresence, bool> filter) => lookup[targetEnum] = filter; // this assignment allows duplicates (which is good)

	public static Func<SpaceStateWithPresence,bool> Get(string filterEnum) => lookup.ContainsKey(filterEnum) 
		? lookup[filterEnum] 
		: throw new ArgumentException( "Unexpected filter:" + filterEnum, nameof( filterEnum ) );

	public static Func<SpaceState, bool> MatchAny( Spirit focusSpirit, TerrainMapper tm, params string[] filterEnums ) {
		return new FilterHelper(focusSpirit,tm, filterEnums ).Any;
	}

	class FilterHelper {
		readonly TerrainMapper mapper;
		readonly Spirit focusSpirit;
		readonly Func<SpaceStateWithPresence, bool>[] filters;
		public FilterHelper( Spirit focusSpirit, TerrainMapper mapper, string[] filterEnums ) {
			this.focusSpirit = focusSpirit;
			this.mapper = mapper;
			var filterEnum = filterEnums.Length == 0 ? Target.Any : filterEnums[0];
			filters = filterEnums.Select( SpaceFilterMap.Get ).ToArray();
		}
		// Same as OR
		public bool Any( SpaceState state ) {
			if( !mapper.IsInPlay( state ) ) return false;
			var sswp = new SpaceStateWithPresence( state, focusSpirit, mapper );
			if( filters.Length == 0 ) return true;
			if( filters.Length == 1 ) return filters[0]( sswp );
			foreach(var filter in filters )
				if( filter( sswp ) ) return true;
			return false;
		}
	}

}

/// <summary>
/// Binds Spirit to SpaceState so we evaluate against the named-criteria
/// </summary>
public class SpaceStateWithPresence {
	public SpaceState Tokens { get; }
	public TerrainMapper TerrainMapper { get; }
	public bool IsPresent => focusSpirit.Presence.IsOn( Tokens );
	public bool MatchesTerrain(Terrain terrain) =>TerrainMapper.MatchesTerrain(Tokens, terrain);
	public bool IsCoastal => TerrainMapper.IsCoastal( Tokens );

	readonly Spirit focusSpirit;
	public SpaceStateWithPresence(SpaceState spaceState, Spirit focusSpirit, TerrainMapper tm) {
		Tokens = spaceState;
		this.focusSpirit = focusSpirit;
		TerrainMapper = tm;
	}
}
