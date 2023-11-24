namespace SpiritIsland;

public abstract class TargetSpaceAttribute : GeneratesContextAttribute {

	public static SpaceState TargettedSpace => _targettedSpace.Value;
	readonly static ActionScopeValue<SpaceState> _targettedSpace = new ActionScopeValue<SpaceState>("Targetted Space");

	readonly protected TargetingSourceCriteria _sourceCriteria;

	protected readonly string[] _targetFilters;
	protected readonly int _range;
	public override string TargetFilterName { get; }

	public TargetSpaceAttribute(TargetingSourceCriteria sourceCriteria, int range, params string[] targetFilter ){
		// Source
		_sourceCriteria = sourceCriteria;
		// Destination - lazy-evaluate later, because some are spirit-dependent
		_range = range;
		_targetFilters = targetFilter;
		// display
		TargetFilterName = 0 < targetFilter.Length ? string.Join( " Or ", targetFilter ) : "Any";
	}

	public override async Task<object> GetTargetCtx( string powerName, SelfCtx ctx ){

		Space space = await ctx.Self.TargetsSpace( ctx, 
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
