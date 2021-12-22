﻿using System;
using System.Collections.Generic;

namespace SpiritIsland {

	public class SpaceFilterMap {

		static readonly Dictionary<string,Func<TargetSpaceCtx, bool>> lookup = new Dictionary<string, Func<TargetSpaceCtx, bool>> {
			[Target.Any               ] = _   => true,
			[Target.Jungle            ] = ctx => ctx.IsOneOf( Terrain.Jungle ),
			[Target.Wetland           ] = ctx => ctx.IsOneOf( Terrain.Wetland ),
			[Target.Mountain          ] = ctx => ctx.IsOneOf( Terrain.Mountain ),
			[Target.JungleOrMountain  ] = ctx => ctx.IsOneOf( Terrain.Jungle, Terrain.Mountain ),
			[Target.JungleOrSand      ] = ctx => ctx.IsOneOf( Terrain.Jungle, Terrain.Sand ),
			[Target.JungleOrWetland   ] = ctx => ctx.IsOneOf( Terrain.Jungle, Terrain.Wetland ),
			[Target.NotWetland        ] = ctx => ctx.IsOneOf( Terrain.Jungle, Terrain.Mountain, Terrain.Sand ),
			[Target.MountainOrWetland ] = ctx => ctx.IsOneOf( Terrain.Mountain, Terrain.Wetland ),
			[Target.MountainOrSand    ] = ctx => ctx.IsOneOf( Terrain.Mountain, Terrain.Sand ),
			[Target.SandOrWetland     ] = ctx => ctx.IsOneOf( Terrain.Sand, Terrain.Wetland ),
			[Target.Coastal           ] = ctx => ctx.IsCoastal,
			[Target.Blight            ] = ctx => ctx.HasBlight,
			[Target.Dahan             ] = ctx => ctx.Dahan.Any,
			[Target.Disease           ] = ctx => ctx.Disease.Any,
			[Target.Invaders          ] = ctx => ctx.HasInvaders,
			[Target.DahanOrInvaders   ] = ctx => ctx.Dahan.Any || ctx.HasInvaders,
			[Target.NoInvader         ] = ctx => !ctx.HasInvaders,
			[Target.NoBlight          ] = ctx => !ctx.HasBlight,
			[Target.TownCityOrBlight  ] = ctx => ctx.HasBlight || ctx.Tokens.HasAny( Invader.City, Invader.Town ),
			[Target.TownOrExplorer    ] = ctx => ctx.Tokens.HasAny( Invader.Explorer, Invader.Town ),  // Wash Away helper
			[Target.TownOrCity        ] = ctx => ctx.Tokens.HasAny( Invader.City, Invader.Town ),  // Study the Invaders' Fears

			// Register new filters needed for Branch and Claw
			[Target.Beast             ] = ctx => ctx.Beasts.Any,
			[Target.BeastOrJungle     ] = ctx => ctx.IsOneOf( Terrain.Jungle ) || ctx.Beasts.Any,
			[Target.PresenceOrWilds   ] = ctx => ctx.IsPresent || ctx.Wilds > 0,
			[Target.CoastalOrWetlands ] = ctx => ctx.IsOneOf( Terrain.Wetland ) || ctx.IsCoastal,
			[Target.City              ] = ctx => ctx.Tokens.Has(Invader.City),

			// Jagged Earth
			[Target.TwoBeasts         ] = ctx => ctx.Beasts.Count>=2,
			[Target.MountainOrPresence] = ctx => ctx.IsOneOf( Terrain.Mountain ) || ctx.HasSelfPresence,

			// Don't use TerrainMapper, Inland should ignore terrain modifications (I think)
			[Target.Inland            ] = ctx => !ctx.Space.IsOcean && !ctx.Space.IsCoastal,

		};

		/// <remarks>Can NOT call this from Cards Static constructor because it is not invoked until AFTER the attribute is evaluated</remarks>
		public static void Register(string targetEnum,Func<TargetSpaceCtx, bool> filter) => lookup[targetEnum] = filter; // this assignment allows duplicates (which is good)

		public static Func<TargetSpaceCtx,bool> Get(string filterEnum) => lookup.ContainsKey(filterEnum) 
			? lookup[filterEnum] 
			: throw new ArgumentException( "Unexpected filter:" + filterEnum, nameof( filterEnum ) );

	}

}
