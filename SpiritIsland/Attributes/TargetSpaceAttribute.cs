using System;
using System.Threading.Tasks;

namespace SpiritIsland {

	public abstract class TargetSpaceAttribute : GeneratesContextAttribute {

		readonly From fromSourceEnum;
		readonly protected Terrain? sourceTerrain;
		readonly protected int range;
		public override string TargetFilter { get; }

		public TargetSpaceAttribute(From source, Terrain? sourceTerrain, int range, string targetFilter){
			this.fromSourceEnum = source;
			this.sourceTerrain = sourceTerrain;
			this.range = range;
			this.TargetFilter = targetFilter;
		}

		public override async Task<object> GetTargetCtx( string powerName, SpiritGameStateCtx ctx, PowerType powerType ){
			var space = await ctx.Self.TargetLandApi.TargetsSpace( ctx.Self, ctx.GameState, powerName+": Target Space", fromSourceEnum, sourceTerrain, await CalcRange(ctx), TargetFilter, powerType );
			return space == null ? null : ctx.Target(space);
		}

		protected virtual Task<int> CalcRange( SpiritGameStateCtx ctx ) => Task.FromResult(range);

		public override LandOrSpirit LandOrSpirit => LandOrSpirit.Land;

	}

	[AttributeUsage( AttributeTargets.Class | AttributeTargets.Method )]
	public class FromPresenceAttribute : TargetSpaceAttribute {
		public FromPresenceAttribute( int range, string filter = Target.Any )
			: base( From.Presence, null, range, filter ) {
		}
		public override string RangeText => range.ToString();
	}

	[AttributeUsage( AttributeTargets.Class | AttributeTargets.Method )]
	public class FromPresenceInAttribute : TargetSpaceAttribute {
		public FromPresenceInAttribute( int range, Terrain sourceTerrain, string filter = Target.Any )
			: base( From.Presence, sourceTerrain, range, filter ) {}
		public override string RangeText => $"{range}({sourceTerrain})";
	}

	[AttributeUsage( AttributeTargets.Class | AttributeTargets.Method )]
	public class FromSacredSiteAttribute : TargetSpaceAttribute {
		public FromSacredSiteAttribute( int range, string filter = Target.Any )
			: base( From.SacredSite, null, range, filter ) { }
		public override string RangeText => $"ss:{range}";
	}

}
