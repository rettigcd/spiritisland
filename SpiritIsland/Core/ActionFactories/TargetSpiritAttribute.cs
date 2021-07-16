using System;
using System.Threading.Tasks;

namespace SpiritIsland.Core {

	[AttributeUsage(AttributeTargets.Class|AttributeTargets.Method)]
	class TargetSpiritAttribute : Attribute {
	}

	interface ITargetSpace{
		Task<Space> Target(Spirit self,PowerCardApi api);
	}

	[AttributeUsage(AttributeTargets.Class|AttributeTargets.Method)]
	class FromPresenceAttribute : Attribute, ITargetSpace {
		readonly int range;
		public FromPresenceAttribute(int range) {
			this.range = range;
		}
		public Task<Space> Target(Spirit self,PowerCardApi api){
			return api.TargetSpace(self.Presence,range);
		}
	}

	[AttributeUsage(AttributeTargets.Class|AttributeTargets.Method)]
	class FromSacredSiteAttribute : Attribute, ITargetSpace {
		readonly int range;
		public FromSacredSiteAttribute(int range) {
			this.range = range;
		}
		public Task<Space> Target(Spirit self,PowerCardApi api){
			return api.TargetSpace(self.SacredSites,range);
		}
	}


}
