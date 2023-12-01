namespace SpiritIsland;

public abstract class TargetSpaceAttribute : GeneratesContextAttribute {

	public static SpaceState TargettedSpace => _targettedSpace.Value;
	readonly static ActionScopeValue<SpaceState> _targettedSpace = new ActionScopeValue<SpaceState>("Targetted Space");

	protected TargetingSourceCriteria _sourceCriteria => new TargetingSourceCriteria(_from,_restrictFrom);
	readonly TargetFrom _from;
	protected readonly string _restrictFrom; 

	protected readonly string[] _targetFilters;
	protected readonly int _range;
	public override string TargetFilterName { get; }

	public TargetSpaceAttribute(TargetFrom from, int range, params string[] targetFilter )
		:this(from,null,range,targetFilter)
	{}

	/// <param name="commaDelimitedRestrictFrom">null or comma-delimited Target</param>
	public TargetSpaceAttribute(TargetFrom from, string commaDelimitedRestrictFrom, int range, params string[] targetFilter ){
		// Source
		_from = from;
		_restrictFrom = commaDelimitedRestrictFrom;
		// Destination - lazy-evaluate later, because some are spirit-dependent
		_range = range;
		_targetFilters = targetFilter;
		// display
		TargetFilterName = 0 < targetFilter.Length ? string.Join( "/", targetFilter ) : "Any";
	}

	public override async Task<object> GetTargetCtx( string powerName, Spirit self ){

		Space space = await self.TargetsSpace( 
			powerName+": Target Space", 
			Preselect,
			_sourceCriteria,
			await GetCriteria( self )
		);
		if(space == null) return null;
		var target = self.Target( space );
		_targettedSpace.Value = target.Tokens;
		return target;
	}

	protected virtual async Task<TargetCriteria> GetCriteria( Spirit self ) 
		=> new TargetCriteria( await CalcRange( self ), self, _targetFilters );

	/// <remarks>Hook so ExtendableRangeAttribute can increase range.</remarks>
	protected virtual Task<int> CalcRange( Spirit self ) => Task.FromResult( _range );

	public override LandOrSpirit LandOrSpirit => LandOrSpirit.Land;

	public IPreselect Preselect {get; set; }

}
