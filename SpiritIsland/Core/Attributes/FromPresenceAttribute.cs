using System;
using System.Collections.Generic;

namespace SpiritIsland.Core {
	[AttributeUsage(AttributeTargets.Class|AttributeTargets.Method)]
	class FromPresenceAttribute : TargetSpaceAttribute {
		public FromPresenceAttribute(int range,Filter filter = Filter.None)
			:base(range,filter){}
		protected override IEnumerable<Space> PickSourceFrom(Spirit self) => self.Presence.Spaces;
	}

}
