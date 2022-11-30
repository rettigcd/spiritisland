namespace SpiritIsland;

public class SpaceFilterMap {

	static readonly Dictionary<string,Func<SpaceStateWithPresence, bool>> lookup = new Dictionary<string, Func<SpaceStateWithPresence, bool>> {
		[Target.Any               ] = (_)   => true,
		[Target.Jungle            ] = (ctx) => ctx.Tokens.Space.IsJungle,
		[Target.Wetland           ] = (ctx) => ctx.Tokens.Space.IsWetland,
		[Target.Mountain          ] = (ctx) => ctx.Tokens.Space.IsMountain,
		[Target.JungleOrMountain  ] = (ctx) => ctx.TerrainMapper.MatchesTerrain( ctx.Tokens, Terrain.Jungle, Terrain.Mountain ),
		[Target.JungleOrSand      ] = (ctx) => ctx.TerrainMapper.MatchesTerrain( ctx.Tokens, Terrain.Jungle, Terrain.Sand ),
		[Target.JungleOrWetland   ] = (ctx) => ctx.TerrainMapper.MatchesTerrain( ctx.Tokens, Terrain.Jungle, Terrain.Wetland ),
		[Target.NotWetland        ] = (ctx) => ctx.TerrainMapper.MatchesTerrain( ctx.Tokens, Terrain.Jungle, Terrain.Mountain, Terrain.Sand ),
		[Target.MountainOrWetland ] = (ctx) => ctx.TerrainMapper.MatchesTerrain( ctx.Tokens, Terrain.Mountain, Terrain.Wetland ),
		[Target.MountainOrSand    ] = (ctx) => ctx.TerrainMapper.MatchesTerrain( ctx.Tokens, Terrain.Mountain, Terrain.Sand ),
		[Target.SandOrWetland     ] = (ctx) => ctx.TerrainMapper.MatchesTerrain( ctx.Tokens, Terrain.Sand, Terrain.Wetland ),
		[Target.JungleOrNoBlight  ] = (ctx) => ctx.Tokens.Space.IsJungle || !ctx.Tokens.Blight.Any,
		[Target.Coastal           ] = (ctx) => ctx.TerrainMapper.IsCoastal(ctx.Tokens.Space),
		[Target.Blight            ] = (ctx) => ctx.Tokens.Blight.Any,
		[Target.Dahan             ] = (ctx) => ctx.Tokens.Dahan.Any,
		[Target.Disease           ] = (ctx) => ctx.Tokens.Disease.Any,
		[Target.Invaders          ] = (ctx) => ctx.Tokens.HasInvaders(),
		[Target.DahanOrInvaders   ] = (ctx) => ctx.Tokens.Dahan.Any || ctx.Tokens.HasInvaders(),
		[Target.NoInvader         ] = (ctx) => !ctx.Tokens.HasInvaders(),
		[Target.NoBlight          ] = (ctx) => !ctx.Tokens.Blight.Any,
		[Target.TownCityOrBlight  ] = (ctx) => ctx.Tokens.Blight.Any || ctx.Tokens.HasAny( Invader.City, Invader.Town ),
		[Target.TownOrExplorer    ] = (ctx) => ctx.Tokens.HasAny( Invader.Explorer, Invader.Town ),  // Wash Away helper
		[Target.TownOrCity        ] = (ctx) => ctx.Tokens.HasAny( Invader.City, Invader.Town ),  // Study the Invaders' Fears
		[Target.NotOcean          ] = (ctx) => !ctx.Tokens.Space.Is( Terrain.Ocean ), // even when ocean is is play, not allowed 

		// Register new filters needed for Branch and Claw
		[Target.Beast             ] = (ctx) => ctx.Tokens.Beasts.Any,
		[Target.BeastOrJungle     ] = (ctx) => ctx.Tokens.Space.IsJungle || ctx.Tokens.Beasts.Any,
		[Target.PresenceOrWilds   ] = (ctx) => ctx.IsPresent || ctx.Tokens.Wilds.Any,
		[Target.Presence          ] = (ctx) => ctx.IsPresent,
		[Target.CoastalOrWetlands ] = (ctx) => ctx.Tokens.Space.IsWetland || ctx.TerrainMapper.IsCoastal(ctx.Tokens.Space),
		[Target.City              ] = (ctx) => ctx.Tokens.Has(Invader.City),

		// Jagged Earth
		[Target.TwoBeasts         ] = (ctx) => ctx.Tokens.Beasts.Count>=2,
		[Target.MountainOrPresence] = (ctx) => ctx.Tokens.Space.IsMountain || ctx.IsPresent,

		// Don't use TerrainMapper, Inland should ignore terrain modifications (I think)
		[Target.Inland            ] = (ctx) => ctx.TerrainMapper.IsInland( ctx.Tokens.Space ),

	};

	// !!! Problems with reducing input to just Space: Focus Presence
	// We want to reduce this to just SpaceState/Tokens because we don't want to generate an Action, simply for evaluating which we can Target.

	/// <remarks>Can NOT call this from Cards Static constructor because it is not invoked until AFTER the attribute is evaluated</remarks>
	public static void Register(string targetEnum,Func<SpaceStateWithPresence, bool> filter) => lookup[targetEnum] = filter; // this assignment allows duplicates (which is good)

	public static Func<SpaceStateWithPresence,bool> Get(string filterEnum) => lookup.ContainsKey(filterEnum) 
		? lookup[filterEnum] 
		: throw new ArgumentException( "Unexpected filter:" + filterEnum, nameof( filterEnum ) );

	public static Func<SpaceState, bool> Get( string filterEnum, Spirit focusSpirit, TerrainMapper tm ) => new FilterHelper(filterEnum,focusSpirit,tm).Filter;

	class FilterHelper {
		readonly Func<SpaceStateWithPresence, bool> filter;
		readonly TerrainMapper mapper;
		readonly Spirit focusSpirit;
		public FilterHelper( string filterEnum, Spirit focusSpirit, TerrainMapper mapper) {
			filter = SpaceFilterMap.Get( filterEnum );
			this.mapper = mapper;
			this.focusSpirit = focusSpirit;
		}
		public bool Filter( SpaceState state ) => filter( new SpaceStateWithPresence(state,focusSpirit,mapper) );
	}

}

/// <summary>
/// Binds Spirit to SpaceState so we evaluate against the named-criteria
/// </summary>
public class SpaceStateWithPresence {
	public SpaceState Tokens { get; }
	public TerrainMapper TerrainMapper { get; }
	public bool IsPresent => focusSpirit.Presence.IsOn( Tokens );
	readonly Spirit focusSpirit;
	public SpaceStateWithPresence(SpaceState spaceState, Spirit focusSpirit, TerrainMapper tm) {
		Tokens = spaceState;
		this.focusSpirit = focusSpirit;
		TerrainMapper = tm;
	}
}
