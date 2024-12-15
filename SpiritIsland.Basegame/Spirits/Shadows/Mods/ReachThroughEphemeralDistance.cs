namespace SpiritIsland.Basegame;

class ReachThroughEphemeralDistance(Spirit spirit) : Targetter(spirit), IRunWhenTimePasses {

	public const string Name = "Reach Through Ephemeral Distance";
	const string Description = "Once per turn, you may ignore Range. (* Currently, this only works for targetting.)"; 
	// This can be during Growth or for a Power - anything for which there's a Range arrow or the word "Range" is used.
	static public SpecialRule Rule => new SpecialRule( Name, Description );

	static public void InitAspect(Spirit spirit) { spirit.Targetter = new ReachThroughEphemeralDistance(spirit); }

	/// <summary>
	/// Overriden so we can pay 1 energy for targetting out-of-range dahan space
	/// </summary>
	public override async Task<TargetSpaceResults> TargetsSpace(
		string prompt,
		IPreselect preselect,
		TargetingSourceCriteria sourceCriteria,
		params TargetCriteria[] targetCriteria
	) {
		// The spaces we picked
		var target = await base.TargetsSpace(prompt, preselect, sourceCriteria, targetCriteria);

		// If we target
		if( _used && _otherTargets.Contains(target.Space) ) {
			_used = true;
			_otherTargets = [];
			GameState.Current.AddTimePassesAction(this);
		}

		return target;
	}

	public override TargetRoutes GetPowerTargetOptions(TargetingSourceCriteria sourceCriteria, params TargetCriteria[] targetCriteria) {
		var routes = base.GetPowerTargetOptions(sourceCriteria, targetCriteria);
		if( !_used ) {
			_otherTargets = ActionScope.Current.Spaces.Except(routes.Targets).ToArray();
			routes.AddRoutes( routes.Sources.SelectMany(s=>_otherTargets.Select(t=>new TargetRoute(s,t))));
		} else {
			_otherTargets = [];
		}
		return routes;
	}

	#region IRunWhenTimePasses

	bool IRunWhenTimePasses.RemoveAfterRun => true;
	TimePassesOrder IRunWhenTimePasses.Order => TimePassesOrder.Normal;
	Task IRunWhenTimePasses.TimePasses(GameState _) { _used = false; return Task.CompletedTask; }

	#endregion IRunWhenTimePasses

	bool _used;
	Space[] _otherTargets = [];
}
