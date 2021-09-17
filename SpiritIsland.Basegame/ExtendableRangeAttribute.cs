using System;

namespace SpiritIsland.Basegame {

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
	}

}

