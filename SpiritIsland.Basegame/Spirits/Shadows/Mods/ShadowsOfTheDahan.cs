namespace SpiritIsland.Basegame;

class ShadowsOfTheDahan(Spirit spirit) : Targetter(spirit) {

	public const string Name = "Shadows of the Dahan";
	static public SpecialRule Rule => new SpecialRule(
		Name,
		"Whenever you use a power, you may pay 1 energy to target land with Dahan regardless of range."
	);

	static public void RemoveFrom(Spirit spirit) => spirit.Targetter = null;

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
		if( 0 < _spirit.Energy 
			&& TargettedDahanSpace(sourceCriteria, targetCriteria, target)
		) --_spirit.Energy;

		return target;
	}

	bool TargettedDahanSpace(TargetingSourceCriteria sourceCriteria, TargetCriteria[] targetCriteria, TargetSpaceResults target) {
		var normalSpaces = base.GetPowerTargetOptions(sourceCriteria, targetCriteria).Targets;
		bool targettedAnEnergySpace = !normalSpaces.Contains(target.Space);
		return targettedAnEnergySpace;
	}

	public override TargetRoutes GetPowerTargetOptions(TargetingSourceCriteria sourceCriteria, params TargetCriteria[] targetCriteria) {
		var routes = base.GetPowerTargetOptions(sourceCriteria, targetCriteria);
		if( 0 < _spirit.Energy ) {
			var dahanSpaces = ActionScope.Current.Spaces.Where(s => s.Dahan.Any).ToArray();
			var dahanRoutes = routes.Sources.SelectMany(s => dahanSpaces.Select(t => new TargetRoute(s, t)));
			routes.AddRoutes(dahanRoutes);
		}
		return routes;
	}

}
