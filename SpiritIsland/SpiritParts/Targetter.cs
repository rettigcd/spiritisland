namespace SpiritIsland;

public class Targetter(Spirit spirit) {

	#region constructor

	#endregion constructor

	/// <summary> Used EXCLUSIVELY For Targeting a PowerCard's Space </summary>
	/// <remarks> This used as the hook for Shadow's Pay-1-to-target-land-with-dahan </remarks>
	public virtual async Task<TargetSpaceResults> TargetsSpace(
		string prompt,
		IPreselect preselect,
		TargetingSourceCriteria sourceCriteria,
		params TargetCriteria[] targetCriteria
	) {
		// Get Options
		var routes = GetPowerTargetOptions(sourceCriteria, targetCriteria);
		var spaceOptions = routes.Targets;

		// 0
		if( spaceOptions.Length == 0 ) {
			ActionScope.Current.LogDebug($"{prompt} => No elligible spaces found!"); // show in debug window why nothing happened.
			return null;
		}

		// 1
		if( spaceOptions.Length == 1 && targetCriteria.Length == 1 && targetCriteria[0].AutoSelectSingle ) {
			// Make sure we still go through SelectAsync<> so we can trigger the selection-made event
			var space = await _spirit.SelectAsync(new A.SpaceDecision(prompt, spaceOptions, Present.AutoSelectSingle));
			return routes.MakeResult(space);
		}

		// multiple Choose
		Space mySpace = preselect != null && UserGateway.UsePreselect.Value
			? await preselect.PreSelect(_spirit, spaceOptions)
			: await _spirit.SelectAsync(new A.SpaceDecision(prompt, spaceOptions, Present.Always));

		// return result.
		return routes.MakeResult(mySpace);
	}

	public virtual TargetRoutes GetPowerTargetOptions(TargetingSourceCriteria sourceCriteria, params TargetCriteria[] targetCriteria) {

		var sources = sourceCriteria.GetSources(_spirit).ToArray();

		return targetCriteria.Length switch {
			0 => new TargetRoutes([]),
			1 => _spirit.PowerRangeCalc.GetTargetingRoute_MultiSpace(sources, targetCriteria[0]),
			_ => new TargetRoutes(targetCriteria.SelectMany(
				tc => _spirit.PowerRangeCalc.GetTargetingRoute_MultiSpace(sources, tc)._routes
			))
		};
	}

	#region private fields

	readonly protected Spirit _spirit = spirit;

	#endregion
}
