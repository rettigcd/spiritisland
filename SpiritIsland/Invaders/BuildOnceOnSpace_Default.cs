namespace SpiritIsland;

/// <summary> 
/// If no Build-Stoppers prevent it, Performs 1 build on 1 space.
/// </summary>
/// <remarks>
/// Only call directly from the build engine.
/// Do NOT use directly.  Instead, go through the build engine when doing builds as this behavior may be overriden by France or Habsburg
/// </remarks>
public class BuildOnceOnSpace_Default { 

	public async Task ActAsync( SpaceState tokens ) {
		_tokens = tokens;
		string buildResult = await GetResult();
		ActionScope.Current.Log( new Log.InvaderActionEntry( _tokens.Space.Label + ": " + buildResult ) );
	}

	async Task<string> GetResult() {

		await using var actionScope = await ActionScope.Start(ActionCategory.Invader);

		// Determine type to build
		var (countToAdd, invaderToAdd) = DetermineWhatToAdd();
		BuildEngine.InvaderToAdd.Value = invaderToAdd;

		// Check for Stoppers
		var buildStoppers = _tokens.ModsOfType<ISkipBuilds>()
			.OrderBy( t => t.Cost ) // cheapest first
			.ToArray();
		foreach(ISkipBuilds stopper in buildStoppers)
			if(await stopper.Skip( _tokens ))
				return "build stopped by " + stopper.Text;

		// build it
		await _tokens.AddDefault( invaderToAdd, countToAdd, AddReason.Build );
		return invaderToAdd.Label;
	}

	protected virtual (int,HumanTokenClass) DetermineWhatToAdd() {
		int townCount = _tokens.Sum( Human.Town );
		int cityCount = _tokens.Sum( Human.City );
		HumanTokenClass invaderToAdd = cityCount < townCount ? Human.City : Human.Town;
		int countToAdd = 1;
		return (countToAdd, invaderToAdd);
	}

	protected SpaceState _tokens;

}