using SpiritIsland.BranchAndClaw;
using System;
using System.Collections.Generic;

namespace SpiritIsland {

	public class SpaceFilter {

		static public readonly SpaceFilter ForPlacingPresence = new SpaceFilter();
		static public readonly SpaceFilter ForCascadingBlight = new UsesSpecialTerrain();
		static public readonly SpaceFilter ForPowers          = new UsesSpecialTerrain(); // !!! Power Push should use this also.

		public static readonly Dictionary<string,Func<SpaceFilterCtx, Space,bool>> lookup = new Dictionary<string, Func<SpaceFilterCtx, Space, bool>> {
			[Target.Any               ] = ( _, s ) => true,
			[Target.SandOrWetland     ] = ( ctx, s ) => ctx.SelectTerrain( s ).IsIn( Terrain.Sand, Terrain.Wetland ),
			[Target.Jungle            ] = ( ctx, s ) => ctx.SelectTerrain( s ) == Terrain.Jungle,
			[Target.Wetland           ] = ( ctx, s ) => ctx.SelectTerrain( s ) == Terrain.Wetland,
			[Target.Mountain          ] = ( ctx, s ) => ctx.SelectTerrain( s ) == Terrain.Mountain,
			[Target.JungleOrMountain  ] = ( ctx, s ) => ctx.SelectTerrain( s ).IsIn( Terrain.Jungle, Terrain.Mountain ),
			[Target.JungleOrSand      ] = ( ctx, s ) => ctx.SelectTerrain( s ).IsIn( Terrain.Jungle, Terrain.Sand ),
			[Target.JungleOrWetland   ] = ( ctx, s ) => ctx.SelectTerrain( s ).IsIn( Terrain.Jungle, Terrain.Wetland ),
			[Target.MountainOrWetland ] = ( ctx, s ) => ctx.SelectTerrain( s ).IsIn( Terrain.Mountain, Terrain.Wetland ),
			[Target.Costal            ] = ( _, s ) => s.IsCostal,
			[Target.NoInvader         ] = ( ctx, s ) => !ctx.GameState.HasInvaders( s ),
			[Target.Blight            ] = ( ctx, s ) => ctx.GameState.HasBlight( s ),
			[Target.NoBlight          ] = ( ctx, s ) => !ctx.GameState.HasBlight( s ),
			[Target.DahanOrInvaders   ] = ( ctx, s ) => (ctx.GameState.DahanIsOn( s ) || ctx.GameState.Tokens[s].HasInvaders()),
			[Target.Explorer          ] = ( ctx, s ) => ctx.GameState.Tokens[ s ].Has(Invader.Explorer),
			[Target.TownOrExplorer    ] = ( ctx, s ) => ctx.GameState.Tokens[ s ].HasAny( Invader.Explorer, Invader.Town ),
			[Target.Dahan             ] = ( ctx, s ) => ctx.GameState.DahanIsOn(s),
			[Target.Invaders          ] = ( ctx, s ) => ctx.GameState.HasInvaders(s),
			// !!! These 2 need to be removed from base game and added via strings
			[Target.BeastOrJungle     ] = ( ctx, s ) => ctx.SelectTerrain( s ) == Terrain.Jungle || ctx.GameState.Tokens[s][BacTokens.Beast]>0,
			[Target.PresenceOrWilds   ] = ( ctx, s ) => (ctx.Spirit.Presence.IsOn( s ) || ctx.GameState.Tokens[s].Has(BacTokens.Wilds)),
			[Target.Inland            ] = ( ctx, s ) => s.Terrain != Terrain.Ocean && !s.IsCostal // Don't use SelectTerrain because even if ocean is Wetland, it is not inland.
		};

		public Func<Space, Terrain> TerrainMapper = (Space space) => space.Terrain;

		class UsesSpecialTerrain : SpaceFilter {
			public UsesSpecialTerrain() { TerrainMapper = ( Space space ) => space.TerrainForPower; }
		}

		public Func<Space, bool> GetFilter( Spirit self, GameState gameState, string filterEnum ) {

			return new SpaceFilterCtx { 
				Spirit = self, 
				GameState = gameState,
				SelectTerrain = TerrainMapper,
				baseFilter = lookup.ContainsKey(filterEnum) 
					? lookup[filterEnum]
					: throw new ArgumentException( "Unexpected filter", nameof( filterEnum ) )
			}.Match;
		}

	}

	public class SpaceFilterCtx {
		public Spirit Spirit;
		public GameState GameState;
		public Func<Space,Terrain> SelectTerrain;
		public Func<SpaceFilterCtx, Space, bool> baseFilter;
		public bool Match(Space space) => baseFilter(this,space) && SelectTerrain(space) != Terrain.Ocean;
	}

	public enum FilterPurpose { Power, Blight, PlacingPresence }

}
