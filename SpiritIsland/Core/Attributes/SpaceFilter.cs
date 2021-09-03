﻿using SpiritIsland.BranchAndClaw;
using System;
using System.Collections.Generic;

namespace SpiritIsland {

	public class SpaceFilter {

		static public readonly SpaceFilter ForPlacingPresence = new SpaceFilter();
		static public readonly SpaceFilter ForCascadingBlight = new UsesSpecialTerrain();
		static public readonly SpaceFilter ForPowers          = new UsesSpecialTerrain();

		protected virtual Terrain SelectTerrain(Space space) => space.Terrain;

		class UsesSpecialTerrain : SpaceFilter {
			protected override Terrain SelectTerrain( Space space ) => space.TerrainForPower;
		}

		public Func<Space, bool> GetFilter( Spirit self, GameState gameState, Target filterEnum ) {

			Dictionary<Target,Func<Space,bool>> lookup = new Dictionary<Target, Func<Space, bool>> {
				[Target.Any               ]= ( s ) => true,
				[Target.SandOrWetland     ]= ( s ) => SelectTerrain( s ).IsIn( Terrain.Sand, Terrain.Wetland ),
				[Target.Jungle            ]= ( s ) => SelectTerrain( s ) == Terrain.Jungle,
				[Target.Wetland           ]= ( s ) => SelectTerrain( s ) == Terrain.Wetland,
				[Target.JungleOrMountain  ]= ( s ) => SelectTerrain( s ).IsIn( Terrain.Jungle, Terrain.Mountain ),
				[Target.JungleOrSand      ]= ( s ) => SelectTerrain( s ).IsIn( Terrain.Jungle, Terrain.Sand ),
				[Target.JungleOrWetland   ]= ( s ) => SelectTerrain( s ).IsIn( Terrain.Jungle, Terrain.Wetland ),
				[Target.MountainOrWetland ]= ( s ) => SelectTerrain( s ).IsIn( Terrain.Mountain, Terrain.Wetland ),
				[Target.NoInvader         ]= ( s ) => !gameState.HasInvaders( s ),
				[Target.Blight            ]= ( s ) => gameState.HasBlight( s ),
				[Target.NoBlight          ]= ( s ) => !gameState.HasBlight( s ),
				[Target.DahanOrInvaders   ]= ( s ) => (gameState.DahanIsOn( s ) || gameState.Tokens[s].HasInvaders()),
				[Target.Costal            ]= ( s ) => s.IsCostal,
				[Target.Explorer          ]= ( s ) => gameState.Tokens[ s ].Has(Invader.Explorer),
				[Target.TownOrExplorer    ]= ( s ) => gameState.Tokens[ s ].HasAny( Invader.Explorer, Invader.Town ),
				[Target.Dahan             ]= gameState.DahanIsOn,
				[Target.Invaders          ]= gameState.HasInvaders,
				// !!! These 2 need to be removed from base game and added via strings
				[Target.BeastOrJungle     ]= ( s ) => SelectTerrain( s ) == Terrain.Jungle || gameState.Tokens[s][BacTokens.Beast]>0,
				[Target.PresenceOrWilds   ]= ( s ) => (self.Presence.IsOn( s ) || gameState.Tokens[s].Has(BacTokens.Wilds))
			};
			var baseFilter = lookup.ContainsKey(filterEnum) 
				? lookup[filterEnum]
				: throw new ArgumentException( "Unexpected filter", nameof( filterEnum ) );

			//Predicate<Space> baseFilter = filterEnum switch {
			//	Target.Any               => ( s ) => true,
			//	Target.SandOrWetland     => ( s ) => SelectTerrain( s ).IsIn( Terrain.Sand, Terrain.Wetland ),
			//	Target.Jungle            => ( s ) => SelectTerrain( s ) == Terrain.Jungle,
			//	Target.Wetland           => ( s ) => SelectTerrain( s ) == Terrain.Wetland,
			//	Target.JungleOrMountain  => ( s ) => SelectTerrain( s ).IsIn( Terrain.Jungle, Terrain.Mountain ),
			//	Target.JungleOrWetland   => ( s ) => SelectTerrain( s ).IsIn( Terrain.Jungle, Terrain.Wetland ),
			//	Target.MountainOrWetland => ( s ) => SelectTerrain( s ).IsIn( Terrain.Mountain, Terrain.Wetland ),
			//	Target.NoInvader         => ( s ) => !gameState.Invaders.AreOn( s ),
			//	Target.Blight            => ( s ) => gameState.HasBlight( s ),
			//	Target.NoBlight          => ( s ) => !gameState.HasBlight( s ),
			//	Target.DahanOrInvaders   => ( s ) => (gameState.Dahan.Has( s ) || gameState.Invaders.AreOn( s )),
			//	Target.Costal            => ( s ) => s.IsCostal,
			//	Target.Explorer          => ( s ) => gameState.Invaders.Counts[ s ].Has(Invader1.Explorer),
			//	Target.TownOrExplorer    => ( s ) => gameState.Invaders.Counts[ s ].HasAny( Invader1.Explorer, Invader1.Town ),
			//	Target.Dahan             => gameState.Dahan.Has,
			//	Target.Invaders          => gameState.Invaders.AreOn,
			//	//				Target.BeastOrJungle     => ( s ) => SelectTerrain( s ) == Terrain.Jungle || gameState.Beasts.AreOn( s ),
			//	//				Target.PresenceOrWilds   => ( s ) => (self.Presence.IsOn( s ) || gameState.Wilds.AreOn( s )),
			//	_ => throw new ArgumentException( "Unexpected filter", nameof( filterEnum ) ),
			//};
			return ( s ) => baseFilter( s ) && SelectTerrain( s ) != Terrain.Ocean;
		}

	}

	public enum FilterPurpose { Power, Blight, PlacingPresence }

}
