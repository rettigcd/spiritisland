using System;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	/// <summary>
	/// Targets a space, but conditionally has an extended reange if certain Elemental thresholds are reached
	/// </summary>
	[AttributeUsage( AttributeTargets.Class | AttributeTargets.Method )]
	public class ExtendableRangeAttribute : TargetSpaceAttribute {
		readonly string triggeringElements;
		readonly int extension;
		public ExtendableRangeAttribute( From from, int range, string targetType, string triggeringElements, int extention ) : base( from, null, range, targetType ) {
			this.triggeringElements = triggeringElements;
			this.extension = extention;
		}

		public override string RangeText => "+"+extension;

		protected override async Task<int> CalcRange( SpiritGameStateCtx ctx ) => range
			+ (await ctx.YouHave( triggeringElements ) ? extension : 0);

		public override LandOrSpirit LandOrSpirit => LandOrSpirit.Land;

	}

}

