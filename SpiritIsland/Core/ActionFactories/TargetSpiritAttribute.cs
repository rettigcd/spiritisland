using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SpiritIsland.Core {

	[AttributeUsage(AttributeTargets.Class|AttributeTargets.Method)]
	class TargetSpiritAttribute : Attribute {}

	abstract class TargetSpace : Attribute {
		readonly int range;
		readonly Filter filterEnum;

		public TargetSpace(int range,Filter filter){
			this.range = range;
			this.filterEnum = filter;
		}

		public Task<Space> Target(ActionEngine engine){
			var (self,gameState)=engine;

			bool HasExplorer(Space space) => gameState.InvadersOn(space).HasExplorer;
			bool TownOrExplorer(Space space) => gameState.InvadersOn(space).FilterBy(Invader.Explorer,Invader.Town).Length>0;

			Func<Space,bool> filter = this.filterEnum switch{
				Filter.Dahan => gameState.HasDahan,
				Filter.Explorer => HasExplorer,
				Filter.TownOrExplorer => TownOrExplorer,
				Filter.Invader => gameState.HasInvaders,
				Filter.Costal => (s=>s.IsCostal),
				Filter.JungleOrMountain => ((space)=>space.Terrain.IsIn(Terrain.Jungle,Terrain.Mountain)),
				Filter.JungleOrWetland => ((space)=>space.Terrain.IsIn(Terrain.Jungle,Terrain.Wetland)),
				Filter.NoBlight => (s=>!gameState.HasBlight(s)),
				_ => null
			};
			return engine.Api.TargetSpace(self.SacredSites,range,filter);
		}
		abstract protected IEnumerable<Space> Source(Spirit spirit);
	}

	[AttributeUsage(AttributeTargets.Class|AttributeTargets.Method)]
	class FromPresenceAttribute : TargetSpace {
		public FromPresenceAttribute(int range,Filter filter = Filter.None)
			:base(range,filter){}
		protected override IEnumerable<Space> Source(Spirit self) => self.Presence;
	}

	[AttributeUsage(AttributeTargets.Class|AttributeTargets.Method)]
	class FromSacredSiteAttribute : TargetSpace {
		public FromSacredSiteAttribute(int range, Filter filter = Filter.None)
			:base(range,filter){}
		protected override IEnumerable<Space> Source(Spirit self) => self.SacredSites;
	}

	enum Filter{
		None,
		Dahan,
		Explorer,
		TownOrExplorer,
		Invader,
		Costal,
		JungleOrMountain,
		JungleOrWetland,
		NoBlight,
	}

}
