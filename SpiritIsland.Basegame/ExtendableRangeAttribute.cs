using System;

namespace SpiritIsland.Basegame {

	// !!! review the usaage of this class.  Should it inherit from  TargetSpace???
	[AttributeUsage( AttributeTargets.Class | AttributeTargets.Method )]
	public class ExtendableRangeAttribute : TargetSpaceAttribute {
		readonly string triggeringElements;
		readonly int extension;
		public ExtendableRangeAttribute( From from, int range, string targetType, string triggeringElements, int extention ) : base( from, null, range, targetType ) {
			this.triggeringElements = triggeringElements;
			this.extension = extention;
		}

		public override string RangeText => "+"+extension;

		protected override int CalcRange( SpiritGameStateCtx ctx ) => range
			+ (ctx.YouHave( triggeringElements ) ? extension : 0);

		public override LandOrSpirit LandOrSpirit => LandOrSpirit.Land;

	}

}

