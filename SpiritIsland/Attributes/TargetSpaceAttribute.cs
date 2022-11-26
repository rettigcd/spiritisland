namespace SpiritIsland;

public abstract class TargetSpaceAttribute : GeneratesContextAttribute {

	//readonly protected From fromSourceEnum;
	//readonly protected Terrain? sourceTerrain;
	readonly protected TargetSourceCriteria sourceCriteria;

	protected readonly int range;
	public override string TargetFilter { get; }

	public TargetSpaceAttribute(TargetSourceCriteria sourceCriteria, int range, string targetFilter ){
		this.sourceCriteria = sourceCriteria;
		this.range = range;
		this.TargetFilter = targetFilter;
	}

	public override async Task<object> GetTargetCtx( string powerName, SelfCtx ctx, TargetingPowerType powerType ){
		var space = await ctx.Self.TargetsSpace( powerType, ctx, powerName+": Target Space",
			sourceCriteria,
			new TargetCriteria( await CalcRange(ctx), TargetFilter )
		);
		return space == null ? null : ctx.Target(space);
	}

	/// <remarks>Hook so ExtendableRangeAttribute can increase range.</remarks>
	protected virtual Task<int> CalcRange( SelfCtx ctx ) => Task.FromResult( range );

	public override LandOrSpirit LandOrSpirit => LandOrSpirit.Land;

}

[AttributeUsage( AttributeTargets.Class | AttributeTargets.Method )]
public class FromPresenceAttribute : TargetSpaceAttribute {
	public FromPresenceAttribute( int range, string filter = Target.Any )
		: base( new TargetSourceCriteria( From.Presence ), range, filter ) {}
	public override string RangeText => range.ToString();
}

[AttributeUsage( AttributeTargets.Class | AttributeTargets.Method )]
public class FromPresenceInAttribute : TargetSpaceAttribute {
	public FromPresenceInAttribute( int range, Terrain sourceTerrain, string filter = Target.Any )
		: base( new TargetSourceCriteria( From.Presence, sourceTerrain), range, filter ) {}
	public override string RangeText => $"{range}({sourceCriteria.Terrain})";
}

[AttributeUsage( AttributeTargets.Class | AttributeTargets.Method )]
public class FromSacredSiteAttribute : TargetSpaceAttribute {
	public FromSacredSiteAttribute( int range, string filter = Target.Any )
		: base( new TargetSourceCriteria( From.SacredSite ), range, filter ) { }
	public override string RangeText => $"ss:{range}";
}
