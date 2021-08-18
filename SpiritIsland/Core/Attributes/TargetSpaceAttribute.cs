using System;
using System.Threading.Tasks;

namespace SpiritIsland {

	abstract class TargetSpaceAttribute : Attribute {

		readonly From source;
		readonly Terrain? sourceTerrain;
		readonly int range;
		readonly Target targetFilter;

		public TargetSpaceAttribute(From source, Terrain? sourceTerrain, int range, Target targetFilter){
			this.source = source;
			this.sourceTerrain = sourceTerrain;
			this.range = range;
			this.targetFilter = targetFilter;
		}

		public Task<Space> GetTarget(ActionEngine engine){
			return engine.TargetSpace( source, sourceTerrain, range, targetFilter );
		}

		static public Func<Space, bool> ToLambda( Spirit self, GameState gameState, Target filterEnum){
			var generalFilter = IncludeOceanToLambda(self, gameState,filterEnum);
			return (space) => generalFilter(space) && space.IsLand; // filter out ocean
		}

		static Func<Space, bool> IncludeOceanToLambda( Spirit self, GameState gameState, Target filterEnum ) {
			bool HasExplorer( Space space ) => gameState.InvadersOn( space ).HasExplorer;
			bool TownOrExplorer( Space space ) => gameState.InvadersOn( space ).FilterBy( Invader.Explorer, Invader.Town ).Length > 0;

			return filterEnum switch {
				Target.Any => ( s ) => true,
				Target.Dahan => gameState.HasDahan,
				Target.Explorer => HasExplorer,
				Target.TownOrExplorer => TownOrExplorer,
				Target.Invaders => gameState.HasInvaders,
				Target.NoInvader => ( s ) => !gameState.HasInvaders( s ),
				Target.Costal => (s => s.IsCostal),
				Target.SandOrWetland => (s => s.Terrain.IsIn( Terrain.Sand, Terrain.Wetland )),

				Target.Jungle => (( space ) => space.Terrain == Terrain.Jungle),
				Target.Wetland => (( space ) => space.Terrain == Terrain.Wetland),

				Target.JungleOrMountain => (( space ) => space.Terrain.IsIn( Terrain.Jungle, Terrain.Mountain )),
				Target.JungleOrWetland => (( space ) => space.Terrain.IsIn( Terrain.Jungle, Terrain.Wetland )),
				Target.MountainOrWetland => (( space ) => space.Terrain.IsIn( Terrain.Mountain, Terrain.Wetland )),

				Target.Blight => (s => gameState.HasBlight( s )),
				Target.NoBlight => (s => !gameState.HasBlight( s )),
				Target.BeastOrJungle => ( s ) => s.Terrain == Terrain.Jungle || gameState.HasBeasts( s ),
				Target.PresenceOrWilds => ( s ) => self.Presence.IsOn( s ) || gameState.HasWilds( s ),
				Target.DahanOrInvaders => ( s ) => gameState.HasDahan( s ) || gameState.HasInvaders( s ),
				_ => throw new ArgumentException( "Unexpected filter", nameof( filterEnum ) ),
			};
		}


	}

	[AttributeUsage( AttributeTargets.Class | AttributeTargets.Method )]
	class FromPresenceAttribute : TargetSpaceAttribute {
		public FromPresenceAttribute( int range, Target filter = Target.Any )
			: base( From.Presence, null, range, filter ) {
		}
	}

	[AttributeUsage( AttributeTargets.Class | AttributeTargets.Method )]
	class FromPresenceInAttribute : TargetSpaceAttribute {
		public FromPresenceInAttribute( int range, Terrain sourceTerrain, Target filter = Target.Any )
			: base( From.Presence, sourceTerrain, range, filter ) {}
	}

	[AttributeUsage( AttributeTargets.Class | AttributeTargets.Method )]
	class FromSacredSiteAttribute : TargetSpaceAttribute {
		public FromSacredSiteAttribute( int range, Target filter = Target.Any )
			: base( From.SacredSite, null, range, filter ) { }
	}

}
