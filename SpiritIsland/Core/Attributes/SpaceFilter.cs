using System;

namespace SpiritIsland {

	public class SpaceFilter {

		static public readonly SpaceFilter ForPlacingPresence = new SpaceFilter();
		static public readonly SpaceFilter ForCascadingBlight = new UsesSpecialTerrain();
		static public readonly SpaceFilter ForPowers          = new UsesSpecialTerrain();

		protected virtual Terrain SelectTerrain(Space space) => space.Terrain;

		class UsesSpecialTerrain : SpaceFilter {
			protected override Terrain SelectTerrain( Space space ) => space.TerrainForPower;
		}

		public Func<Space, bool> GetFilter( Spirit _, GameState gameState, Target filterEnum ) {

			Predicate<Space> baseFilter = filterEnum switch {
				Target.Any               => ( s ) => true,
				Target.SandOrWetland     => ( s ) => SelectTerrain( s ).IsIn( Terrain.Sand, Terrain.Wetland ),
				Target.Jungle            => ( s ) => SelectTerrain( s ) == Terrain.Jungle,
				Target.Wetland           => ( s ) => SelectTerrain( s ) == Terrain.Wetland,
				Target.JungleOrMountain  => ( s ) => SelectTerrain( s ).IsIn( Terrain.Jungle, Terrain.Mountain ),
				Target.JungleOrWetland   => ( s ) => SelectTerrain( s ).IsIn( Terrain.Jungle, Terrain.Wetland ),
				Target.MountainOrWetland => ( s ) => SelectTerrain( s ).IsIn( Terrain.Mountain, Terrain.Wetland ),
				Target.NoInvader         => ( s ) => !gameState.Invaders.AreOn( s ),
				Target.Blight            => ( s ) => gameState.HasBlight( s ),
				Target.NoBlight          => ( s ) => !gameState.HasBlight( s ),
//				Target.BeastOrJungle     => ( s ) => SelectTerrain( s ) == Terrain.Jungle || gameState.Beasts.AreOn( s ),
//				Target.PresenceOrWilds   => ( s ) => (self.Presence.IsOn( s ) || gameState.Wilds.AreOn( s )),
				Target.DahanOrInvaders   => ( s ) => (gameState.Dahan.Has( s ) || gameState.Invaders.AreOn( s )),
				Target.Costal            => ( s ) => s.IsCostal,
				Target.Explorer          => ( s ) => gameState.Invaders.Counts[ s ].Has(Invader.Explorer),
				Target.TownOrExplorer    => ( s ) => gameState.Invaders.Counts[ s ].HasAny( Invader.Explorer, Invader.Town ),
				Target.Dahan             => gameState.Dahan.Has,
				Target.Invaders          => gameState.Invaders.AreOn,
				_                        => throw new ArgumentException( "Unexpected filter", nameof( filterEnum ) ),
			};
			return ( s ) => baseFilter( s ) && SelectTerrain( s ) != Terrain.Ocean;
		}

	}

	public enum FilterPurpose { Power, Blight, PlacingPresence }

}
