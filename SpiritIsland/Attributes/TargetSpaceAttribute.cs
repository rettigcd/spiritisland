namespace SpiritIsland;

public abstract class TargetSpaceAttribute : GeneratesContextAttribute {

	public static SpaceState TargettedSpace => _targettedSpace.Value;
	readonly static ActionScopeValue<SpaceState> _targettedSpace = new ActionScopeValue<SpaceState>("Targetted Space");

	readonly protected TargetingSourceCriteria _sourceCriteria;

	protected readonly string[] _targetFilters;
	protected readonly int _range;
	public override string TargetFilter { get; }

	public TargetSpaceAttribute(TargetingSourceCriteria sourceCriteria, int range, params string[] targetFilter ){
		_sourceCriteria = sourceCriteria;
		_range = range;
		TargetFilter = targetFilter.Length>0 ? string.Join(" Or ",targetFilter) : "Any";
		_targetFilters = targetFilter;
	}

	public override async Task<object> GetTargetCtx( string powerName, SelfCtx ctx ){

		var space = await ctx.Self.TargetsSpace( ctx, 
			powerName+": Target Space", 
			Preselect,
			_sourceCriteria,
			await GetCriteria( ctx )
		);
		if(space == null) return null;
		var target = ctx.Target( space );
		_targettedSpace.Value = target.Tokens;
		return target;
	}

	protected virtual async Task<TargetCriteria> GetCriteria( SelfCtx ctx ) 
		=> new TargetCriteria( await CalcRange( ctx ), ctx.Self, _targetFilters );

	/// <remarks>Hook so ExtendableRangeAttribute can increase range.</remarks>
	protected virtual Task<int> CalcRange( SelfCtx ctx ) => Task.FromResult( _range );

	public override LandOrSpirit LandOrSpirit => LandOrSpirit.Land;

	public IPreselect Preselect {get; set; }

}

[AttributeUsage( AttributeTargets.Class | AttributeTargets.Method )]
public class FromPresenceAttribute : TargetSpaceAttribute {
	public FromPresenceAttribute( int range, params string[] filters )
		: base( new TargetingSourceCriteria( From.Presence ), range, filters ) {}
	public override string RangeText => _range.ToString();
}

[AttributeUsage( AttributeTargets.Class | AttributeTargets.Method )]
public class FromPresenceInAttribute : TargetSpaceAttribute {
	public FromPresenceInAttribute( int range, Terrain sourceTerrain, string filter = Target.Any )
		: base( new TargetingSourceCriteria( From.Presence, sourceTerrain), range, filter ) {}
	public override string RangeText => $"{_range}:{_sourceCriteria.Terrain}";
}

[AttributeUsage( AttributeTargets.Class | AttributeTargets.Method )]
public class FromSacredSiteAttribute : TargetSpaceAttribute {
	public FromSacredSiteAttribute( int range, params string[] filters )
		: base( new TargetingSourceCriteria( From.SacredSite ), range, filters ) { }
	public override string RangeText => $"{_range}:ss";
}

[AttributeUsage( AttributeTargets.Class | AttributeTargets.Method )]
public class FromIncarnaAttribute : TargetSpaceAttribute {
	public FromIncarnaAttribute()
		: base( new TargetingSourceCriteria( From.Incarna ), 0, Target.Incarna ) { }
	public override string RangeText => $"S+";
}