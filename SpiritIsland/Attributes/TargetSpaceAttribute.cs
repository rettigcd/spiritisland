namespace SpiritIsland;

public abstract class TargetSpaceAttribute : GeneratesContextAttribute {

	//readonly protected From fromSourceEnum;
	//readonly protected Terrain? sourceTerrain;
	readonly protected TargetingSourceCriteria sourceCriteria;

	protected readonly int range;
	public override string TargetFilter { get; }
	readonly string[] _targetFilters;

	public TargetSpaceAttribute(TargetingSourceCriteria sourceCriteria, int range, params string[] targetFilter ){
		this.sourceCriteria = sourceCriteria;
		this.range = range;
		TargetFilter = targetFilter.Length>0 ? string.Join("Or",targetFilter) : "Any";
		_targetFilters = targetFilter;
	}

	public override async Task<object> GetTargetCtx( string powerName, SelfCtx ctx, TargetingPowerType powerType ){
		var space = await ctx.Self.TargetsSpace( powerType, ctx, powerName+": Target Space",
			sourceCriteria,
			ctx.TerrainMapper.Specify( await CalcRange(ctx), _targetFilters )
		);
		return space == null ? null : ctx.Target(space);
	}

	/// <remarks>Hook so ExtendableRangeAttribute can increase range.</remarks>
	protected virtual Task<int> CalcRange( SelfCtx ctx ) => Task.FromResult( range );

	public override LandOrSpirit LandOrSpirit => LandOrSpirit.Land;

}

[AttributeUsage( AttributeTargets.Class | AttributeTargets.Method )]
public class FromPresenceAttribute : TargetSpaceAttribute {
	public FromPresenceAttribute( int range, params string[] filters )
		: base( new TargetingSourceCriteria( From.Presence ), range, filters ) {}
	public override string RangeText => range.ToString();
}

[AttributeUsage( AttributeTargets.Class | AttributeTargets.Method )]
public class FromPresenceInAttribute : TargetSpaceAttribute {
	public FromPresenceInAttribute( int range, Terrain sourceTerrain, string filter = Target.Any )
		: base( new TargetingSourceCriteria( From.Presence, sourceTerrain), range, filter ) {}
	public override string RangeText => $"{range}({sourceCriteria.Terrain})";
}

[AttributeUsage( AttributeTargets.Class | AttributeTargets.Method )]
public class FromSacredSiteAttribute : TargetSpaceAttribute {
	public FromSacredSiteAttribute( int range, params string[] filters )
		: base( new TargetingSourceCriteria( From.SacredSite ), range, filters ) { }
	public override string RangeText => $"ss:{range}";
}
