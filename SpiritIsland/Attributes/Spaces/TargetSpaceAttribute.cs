namespace SpiritIsland;

/// <param name="commaDelimitedRestrictFrom">null or comma-delimited Target</param>
public abstract class TargetSpaceAttribute( TargetFrom from, string? commaDelimitedRestrictFrom, int range, string[] targetFilterOptions ) 
	: GeneratesContextAttribute
{

	#region ActionScope Targetted Details
	/// <summary> The space that was targetted (if any) and the targetting criteria used to get it. </summary>
	public static TargetSpaceResults? TargettedSpace => _targettedSpace.Value; // must be nullable so action can detect IF a space was targetted in a round.
	readonly static ActionScopeValueNullable<TargetSpaceResults> _targettedSpace = new ActionScopeValueNullable<TargetSpaceResults>("Targetted Space");

	/// <summary>
	/// For powers that need the land they were targeted from (their origins): <see cref="TargettedSpace"/>, or a clear
	/// error when the power is being used without having been targeted, instead of a NullReferenceException.
	/// </summary>
	public static TargetSpaceResults RequireTargettedSpace() => TargettedSpace
		?? throw new InvalidOperationException( "This power uses the land it was targeted from, but it is being used without having been targeted. Set the details first (see SetTargettedSpace)." );

	/// <summary>
	/// For a power that is used again without being targeted again (Relentless Gaze's repeat): sets the details powers read
	/// from <see cref="TargettedSpace"/> (the origin lands) for the current action.
	/// </summary>
	public static void SetTargettedSpace( TargetSpaceResults results ) => _targettedSpace.Value = results;

	#endregion ActionScope Targetted Details

	public override LandOrSpirit LandOrSpirit => LandOrSpirit.Land;

	public IPreselect? Preselect { get; set; }

	/// <summary>
	/// Whether <see cref="GetTargetOptions"/> says the lands this power can target, without asking the user anything, and
	/// <see cref="GetTargetCtx"/> then accepts one of them as already chosen. False for powers that ask something first (an element
	/// threshold that changes the range) or target in their own way: they are chosen in two steps even when actions and
	/// targets are combined.
	/// </summary>
	public virtual bool CanPreviewTargets => true;

	public override string TargetFilterName { get; } = 0 < targetFilterOptions.Length ? string.Join( "/", targetFilterOptions ) : "Any";

	#region constructor

	public TargetSpaceAttribute(TargetFrom from, int range, params string[] targetFilter )
		:this(from,null,range,targetFilter)
	{}

	#endregion

	public override async Task<object?> GetTargetCtx( string powerName, Spirit self ){

		TargetCriteria[] targetCriteria = await ApplySpiritModsToGetTargetCriteria(self);
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

	/// <summary>
	/// The lands this power could target right now - the options GetTargetCtx would offer - without asking which one.
	/// For previews. Note it may still ask an element-threshold question if the power's range/target depends on one.
	/// </summary>
	public async Task<Space[]> GetTargetOptions( Spirit self )
		=> self.Targetter.GetPowerTargetOptions( _sourceCriteria, await ApplySpiritModsToGetTargetCriteria( self ) ).Targets;

	#region protected fields

	protected TargetingSourceCriteria _sourceCriteria => new TargetingSourceCriteria(from, _restrictFrom);
	protected readonly string? _restrictFrom = commaDelimitedRestrictFrom;
	protected readonly string[] _targetFilterOptions = targetFilterOptions;
	protected readonly int _range = range;


	/// <summary>
	/// Apply Spirit modes to Card attributes to get actual TargetCriteria
	/// </summary>
	/// <returns>Usually one set of criteria. A power with more than one way to be targeted (Carapaced Land: range 0, or range 1 for a land with Beast) returns one for each.</returns>
	protected virtual async Task<TargetCriteria[]> ApplySpiritModsToGetTargetCriteria(Spirit self)
		=> [new TargetCriteria(await CalcRange(self), self, _targetFilterOptions)];

	/// <remarks>Hook so ExtendableRangeAttribute can increase range.</remarks>
	protected virtual Task<int> CalcRange(Spirit self) => Task.FromResult(_range);

	#endregion protected fields

}
