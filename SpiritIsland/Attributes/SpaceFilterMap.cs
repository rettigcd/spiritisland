using System;
using System.Collections.Generic;

namespace SpiritIsland {

	public class SpaceFilterMap {

		static readonly Dictionary<string,Func<TargetSpaceCtx, bool>> lookup = new Dictionary<string, Func<TargetSpaceCtx, bool>> {
			[Target.Any               ] = _   => true,
			[Target.Jungle            ] = ctx => ctx.Terrain == Terrain.Jungle,
			[Target.Wetland           ] = ctx => ctx.Terrain == Terrain.Wetland,
			[Target.Mountain          ] = ctx => ctx.Terrain == Terrain.Mountain,
			[Target.SandOrWetland     ] = ctx => ctx.Terrain.IsOneOf( Terrain.Sand, Terrain.Wetland ),
			[Target.JungleOrMountain  ] = ctx => ctx.Terrain.IsOneOf( Terrain.Jungle, Terrain.Mountain ),
			[Target.JungleOrSand      ] = ctx => ctx.Terrain.IsOneOf( Terrain.Jungle, Terrain.Sand ),
			[Target.JungleOrWetland   ] = ctx => ctx.Terrain.IsOneOf( Terrain.Jungle, Terrain.Wetland ),
			[Target.MountainOrWetland ] = ctx => ctx.Terrain.IsOneOf( Terrain.Mountain, Terrain.Wetland ),
			[Target.Coastal           ] = ctx => ctx.IsCoastal,
			[Target.Blight            ] = ctx => ctx.HasBlight,
			[Target.Dahan             ] = ctx => ctx.HasDahan,
			[Target.Invaders          ] = ctx => ctx.HasInvaders,
			[Target.DahanOrInvaders   ] = ctx => ctx.HasDahan || ctx.HasInvaders,
			[Target.NoInvader         ] = ctx => !ctx.HasInvaders,
			[Target.NoBlight          ] = ctx => !ctx.HasBlight,
			[Target.TownOrExplorer    ] = ctx => ctx.Tokens.HasAny( Invader.Explorer, Invader.Town ),  // Wash Away helper
		};

		/// <remarks>Can NOT call this from Cards Static constructor because it is not invoked until AFTER the attribute is evaluated</remarks>
		public static void Register(string targetEnum,Func<TargetSpaceCtx, bool> filter) => lookup[targetEnum] = filter; // this assignment allows duplicates (which is good)

		public static Func<TargetSpaceCtx,bool> Get(string filterEnum) => lookup.ContainsKey(filterEnum) 
			? lookup[filterEnum] 
			: throw new ArgumentException( "Unexpected filter", nameof( filterEnum ) );

	}

}
