using System;
using System.Collections.Generic;

namespace SpiritIsland.Core {
	[AttributeUsage(AttributeTargets.Class|AttributeTargets.Method)]
	class FromSacredSiteAttribute : TargetSpaceAttribute {
		public FromSacredSiteAttribute(int range, Filter filter = Filter.None)
			:base(range,filter){}
		protected override IEnumerable<Space> Source(Spirit self) => self.SacredSites;
	}

}
