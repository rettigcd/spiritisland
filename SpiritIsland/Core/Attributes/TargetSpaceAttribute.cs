using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.Core {

	abstract class TargetSpaceAttribute : Attribute {
		readonly int range;
		readonly Filter filterEnum;

		public TargetSpaceAttribute(int range,Filter filter){
			this.range = range;
			this.filterEnum = filter;
		}

		public Task<Space> Target(ActionEngine engine){
			var source = PickSourceFrom(engine.Self);
			var filter = ToLambda( engine, filterEnum );
			return engine.Api.TargetSpace(engine,source,range, filter );
		}

		static public Func<Space, bool> ToLambda(ActionEngine engine, Filter filterEnum){
			var (self,gameState) = engine;
			bool HasExplorer( Space space ) => gameState.InvadersOn( space ).HasExplorer;
			bool TownOrExplorer( Space space ) => gameState.InvadersOn( space ).FilterBy( Invader.Explorer, Invader.Town ).Length > 0;

			return filterEnum switch {
				Filter.None => (s)=>true, // Is land?
				Filter.Dahan => gameState.HasDahan,
				Filter.Explorer => HasExplorer,
				Filter.TownOrExplorer => TownOrExplorer,
				Filter.Invader => gameState.HasInvaders,
				Filter.Costal => (s => s.IsCostal),
				Filter.SandOrWetland => (s => s.Terrain.IsIn( Terrain.Sand, Terrain.Wetland )),
				Filter.JungleOrMountain => (( space ) => space.Terrain.IsIn( Terrain.Jungle, Terrain.Mountain )),
				Filter.JungleOrWetland => (( space ) => space.Terrain.IsIn( Terrain.Jungle, Terrain.Wetland )),
				Filter.MountainOrWetland => (( space ) => space.Terrain.IsIn( Terrain.Mountain, Terrain.Wetland )),
				Filter.NoBlight => (s => !gameState.HasBlight( s )),
				Filter.BeastOrJungle => (s) => s.Terrain == Terrain.Jungle || gameState.HasBeasts( s ),
				Filter.PresenceOrWilds => (s) => self.Presence.Contains( s ) || gameState.HasWilds( s ),
				Filter.DahanOrInvaders => (s) => gameState.HasDahan( s ) || gameState.HasInvaders( s ),
			_ => throw new ArgumentException("Unexpected filter",nameof(filterEnum)),
			};
		}

		abstract protected IEnumerable<Space> PickSourceFrom(Spirit spirit);
	}

}
