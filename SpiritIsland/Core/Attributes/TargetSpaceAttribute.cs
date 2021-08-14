using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.Core {

	abstract class TargetSpaceAttribute : Attribute {
		readonly int range;
		readonly Target filterEnum;

		public TargetSpaceAttribute(int range,Target filter){
			this.range = range;
			this.filterEnum = filter;
		}

		public Task<Space> Target(ActionEngine engine){
			var source = PickSourceFrom(engine.Self);
			var filter = ToLambda( engine, filterEnum );
			return engine.Api.TargetSpace(engine,source,range, filter );
		}

		static public Func<Space, bool> ToLambda(ActionEngine engine, Target filterEnum){
			var (self,gameState) = engine;
			bool HasExplorer( Space space ) => gameState.InvadersOn( space ).HasExplorer;
			bool TownOrExplorer( Space space ) => gameState.InvadersOn( space ).FilterBy( Invader.Explorer, Invader.Town ).Length > 0;

			return filterEnum switch {
				Core.Target.Any => (s)=>true, // Is land?
				Core.Target.Dahan => gameState.HasDahan,
				Core.Target.Explorer => HasExplorer,
				Core.Target.TownOrExplorer => TownOrExplorer,
				Core.Target.Invader => gameState.HasInvaders,
				Core.Target.NoInvader => (s)=>!gameState.HasInvaders( s ),
				Core.Target.Costal => (s => s.IsCostal),
				Core.Target.SandOrWetland => (s => s.Terrain.IsIn( Terrain.Sand, Terrain.Wetland )),
				Core.Target.JungleOrMountain => (( space ) => space.Terrain.IsIn( Terrain.Jungle, Terrain.Mountain )),
				Core.Target.JungleOrWetland => (( space ) => space.Terrain.IsIn( Terrain.Jungle, Terrain.Wetland )),
				Core.Target.MountainOrWetland => (( space ) => space.Terrain.IsIn( Terrain.Mountain, Terrain.Wetland )),
				Core.Target.NoBlight => (s => !gameState.HasBlight( s )),
				Core.Target.BeastOrJungle => (s) => s.Terrain == Terrain.Jungle || gameState.HasBeasts( s ),
				Core.Target.PresenceOrWilds => (s) => self.Presence.IsOn( s ) || gameState.HasWilds( s ),
				Core.Target.DahanOrInvaders => (s) => gameState.HasDahan( s ) || gameState.HasInvaders( s ),
			_ => throw new ArgumentException( "Unexpected filter",nameof( filterEnum ) ),
			};
		}

		abstract protected IEnumerable<Space> PickSourceFrom(Spirit spirit);
	}

}
