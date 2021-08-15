using System;
using System.Collections.Generic;

namespace SpiritIsland {

	[AttributeUsage(AttributeTargets.Class|AttributeTargets.Method)]
	class FromPresenceAttribute : TargetSpaceAttribute {
		public FromPresenceAttribute(int range,Target filter = Target.Any)
			:base(range,filter){}
		protected override IEnumerable<Space> PickSourceFrom(Spirit self) => self.Presence.Spaces;
	}

}
