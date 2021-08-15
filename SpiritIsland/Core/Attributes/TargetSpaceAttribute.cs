using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland {

	abstract class TargetSpaceAttribute : Attribute {
		readonly int range;
		readonly Target filterEnum;

		public TargetSpaceAttribute(int range,Target filter){
			this.range = range;
			this.filterEnum = filter;
		}

		public Task<Space> GetTarget(ActionEngine engine){
			var source = PickSourceFrom(engine.Self);
			var filter = ToLambda( engine, filterEnum );
			return engine.Api.TargetSpace(engine,source,range, filter );
		}

		static public Func<Space, bool> ToLambda(ActionEngine engine, Target filterEnum){
			var (self,gameState) = engine;
			bool HasExplorer( Space space ) => gameState.InvadersOn( space ).HasExplorer;
			bool TownOrExplorer( Space space ) => gameState.InvadersOn( space ).FilterBy( Invader.Explorer, Invader.Town ).Length > 0;

			return filterEnum switch {
				Target.Any => (s)=>true, // Is land?
				Target.Dahan => gameState.HasDahan,
				Target.Explorer => HasExplorer,
				Target.TownOrExplorer => TownOrExplorer,
				Target.Invaders => gameState.HasInvaders,
				Target.NoInvader => (s)=>!gameState.HasInvaders( s ),
				Target.Costal => (s => s.IsCostal),
				Target.SandOrWetland => (s => s.Terrain.IsIn( Terrain.Sand, Terrain.Wetland )),
				Target.JungleOrMountain => (( space ) => space.Terrain.IsIn( Terrain.Jungle, Terrain.Mountain )),
				Target.JungleOrWetland => (( space ) => space.Terrain.IsIn( Terrain.Jungle, Terrain.Wetland )),
				Target.MountainOrWetland => (( space ) => space.Terrain.IsIn( Terrain.Mountain, Terrain.Wetland )),
				Target.Blight => (s => gameState.HasBlight( s )),
				Target.NoBlight => (s => !gameState.HasBlight( s )),
				Target.BeastOrJungle => (s) => s.Terrain == Terrain.Jungle || gameState.HasBeasts( s ),
				Target.PresenceOrWilds => (s) => self.Presence.IsOn( s ) || gameState.HasWilds( s ),
				Target.DahanOrInvaders => (s) => gameState.HasDahan( s ) || gameState.HasInvaders( s ),
			_ => throw new ArgumentException( "Unexpected filter",nameof( filterEnum ) ),
			};
		}

		abstract protected IEnumerable<Space> PickSourceFrom(Spirit spirit);
	}

}
