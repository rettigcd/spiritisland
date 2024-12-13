namespace SpiritIsland;

/// <param name="commaDelimitedRestrictFrom">null or comma-delimited Target</param>
public abstract class TargetSpaceAttribute( TargetFrom from, string commaDelimitedRestrictFrom, int range, params string[] targetFilter ) 
	: GeneratesContextAttribute
{

	#region ActionScope Targetted Details
	/// <summary> The space that was targetted (if any) and the targetting criteria used to get it. </summary>
	public static TargetSpaceResults TargettedSpace => _targettedSpace.Value;
	readonly static ActionScopeValue<TargetSpaceResults> _targettedSpace = new ActionScopeValue<TargetSpaceResults>("Targetted Space");

	#endregion ActionScope Targetted Details

	public override LandOrSpirit LandOrSpirit => LandOrSpirit.Land;

	public IPreselect Preselect { get; set; }

	public override string TargetFilterName { get; } = 0 < targetFilter.Length ? string.Join( "/", targetFilter ) : "Any";

	#region constructor

	public TargetSpaceAttribute(TargetFrom from, int range, params string[] targetFilter )
		:this(from,null,range,targetFilter)
	{}

	#endregion

	public override async Task<object> GetTargetCtx( string powerName, Spirit self ){

		TargetCriteria targetCriteria = await ApplySpiritModsToGetTargetCriteria(self);
		var result = await self.Targetter.TargetsSpace( 
			powerName+": Target Space", 
			Preselect,
			_sourceCriteria,
			targetCriteria
		);
		if(result == null) return null;
		TargetSpaceCtx target = self.Target( result.Space );
		// Store ambient targetting Details in case anyone needs it.
		_targettedSpace.Value = result;
		return target;
	}

	#region protected fields

	protected TargetingSourceCriteria _sourceCriteria => new TargetingSourceCriteria(from, _restrictFrom);
	protected readonly string _restrictFrom = commaDelimitedRestrictFrom;
	protected readonly string[] _targetFilters = targetFilter;
	protected readonly int _range = range;


	/// <summary>
	/// Apply Spirit modes to Card attributes to get actual TargetCriteria
	/// </summary>
	protected virtual async Task<TargetCriteria> ApplySpiritModsToGetTargetCriteria(Spirit self)
		=> new TargetCriteria(await CalcRange(self), self, _targetFilters);

	/// <remarks>Hook so ExtendableRangeAttribute can increase range.</remarks>
	protected virtual Task<int> CalcRange(Spirit self) => Task.FromResult(_range);

	#endregion protected fields

}
