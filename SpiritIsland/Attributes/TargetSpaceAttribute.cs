using System;
using System.Threading.Tasks;

namespace SpiritIsland {

	public abstract class TargetSpaceAttribute : Attribute {

		readonly From fromSourceEnum;
		readonly Terrain? sourceTerrain;
		readonly protected int range;
		readonly string targetFilter;

		public TargetSpaceAttribute(From source, Terrain? sourceTerrain, int range, string targetFilter){
			this.fromSourceEnum = source;
			this.sourceTerrain = sourceTerrain;
			this.range = range;
			this.targetFilter = targetFilter;
		}

		public Task<Space> GetTarget( Spirit spirit, GameState gameState ){
			return spirit.PowerApi.TargetsSpace( spirit, gameState, fromSourceEnum, sourceTerrain, range, targetFilter );
		}

		protected virtual int CalcRange( SpiritGameStateCtx ctx ) => range;

	}

	[AttributeUsage( AttributeTargets.Class | AttributeTargets.Method )]
	public class FromPresenceAttribute : TargetSpaceAttribute {
		public FromPresenceAttribute( int range, string filter = Target.Any )
			: base( From.Presence, null, range, filter ) {
		}
	}

	[AttributeUsage( AttributeTargets.Class | AttributeTargets.Method )]
	public class FromPresenceInAttribute : TargetSpaceAttribute {
		public FromPresenceInAttribute( int range, Terrain sourceTerrain, string filter = Target.Any )
			: base( From.Presence, sourceTerrain, range, filter ) {}
	}

	[AttributeUsage( AttributeTargets.Class | AttributeTargets.Method )]
	public class FromSacredSiteAttribute : TargetSpaceAttribute {
		public FromSacredSiteAttribute( int range, string filter = Target.Any )
			: base( From.SacredSite, null, range, filter ) { }
	}

}
