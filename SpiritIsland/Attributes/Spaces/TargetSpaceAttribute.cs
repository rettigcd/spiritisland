namespace SpiritIsland;

/// <param name="commaDelimitedRestrictFrom">null or comma-delimited Target</param>
public abstract class TargetSpaceAttribute( TargetFrom from, string commaDelimitedRestrictFrom, int range, params string[] targetFilter ) : GeneratesContextAttribute {

	public static Space TargettedSpace => _targettedSpace.Value;
	readonly static ActionScopeValue<Space> _targettedSpace = new ActionScopeValue<Space>("Targetted Space");

	protected TargetingSourceCriteria _sourceCriteria => new TargetingSourceCriteria(from,_restrictFrom);

	protected readonly string _restrictFrom = commaDelimitedRestrictFrom; 

	protected readonly string[] _targetFilters = targetFilter;
	protected readonly int _range = range;
	public override string TargetFilterName { get; } = 0 < targetFilter.Length ? string.Join( "/", targetFilter ) : "Any";

	public TargetSpaceAttribute(TargetFrom from, int range, params string[] targetFilter )
		:this(from,null,range,targetFilter)
	{}

	public override async Task<object> GetTargetCtx( string powerName, Spirit self ){

		var targetCriteria = await GetCriteria(self);
		Space space = await self.TargetsSpace( 
			powerName+": Target Space", 
			Preselect,
			_sourceCriteria,
			targetCriteria
		);
		if(space == null) return null;
		TargetSpaceCtx target = self.Target( space );
		_targettedSpace.Value = target.Space;
		return target;
	}

	protected virtual async Task<TargetCriteria> GetCriteria( Spirit self ) 
		=> new TargetCriteria( await CalcRange( self ), self, _targetFilters );

	/// <remarks>Hook so ExtendableRangeAttribute can increase range.</remarks>
	protected virtual Task<int> CalcRange( Spirit self ) => Task.FromResult( _range );

	public override LandOrSpirit LandOrSpirit => LandOrSpirit.Land;

	public IPreselect Preselect {get; set; }

}
