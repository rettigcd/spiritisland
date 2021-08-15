using System;
using System.Collections.Generic;
using System.Linq;

namespace SpiritIsland {
	[AttributeUsage( AttributeTargets.Class | AttributeTargets.Method )]
	class FromPresenceInAttribute : TargetSpaceAttribute {
		readonly Terrain terrain;
		public FromPresenceInAttribute( int range, Terrain terrain, Target filter = Target.Any )
			: base( range, filter ) {
			this.terrain = terrain;
		}
		protected override IEnumerable<Space> PickSourceFrom( Spirit self ) => self.Presence.Spaces.Where( x => x.Terrain == terrain );
	}

}
