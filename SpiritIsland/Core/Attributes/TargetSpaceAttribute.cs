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
			var (self,gameState)=engine;
			var source = PickSourceFrom(self);
			var filter = ToLambda( gameState, filterEnum );
			return engine.Api.TargetSpace(engine,source,range, filter );
		}

		static public Func<Space, bool> ToLambda(GameState gameState, Filter filterEnum){
			bool HasExplorer( Space space ) => gameState.InvadersOn( space ).HasExplorer;
			bool TownOrExplorer( Space space ) => gameState.InvadersOn( space ).FilterBy( Invader.Explorer, Invader.Town ).Length > 0;

			return filterEnum switch {
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
				_ => null
			};
		}

		abstract protected IEnumerable<Space> PickSourceFrom(Spirit spirit);
	}

}
