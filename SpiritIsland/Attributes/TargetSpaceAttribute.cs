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

		public override async Task<object> GetTargetCtx( Spirit spirit, GameState gameState ){
			// !!! replace null with prompt indicating what card we are targetting
			var space = await spirit.PowerApi.TargetsSpace( spirit, gameState, null, fromSourceEnum, sourceTerrain, range, TargetFilter );
			return space == null ? null 
				: new TargetSpaceCtx( spirit, gameState, space, Cause.Power);
		}

		protected virtual int CalcRange( SpiritGameStateCtx ctx ) => range;

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
