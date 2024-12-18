namespace SpiritIsland;

/// <summary> 
/// If no Build-Stoppers prevent it, Performs 1 build on 1 space.
/// </summary>
/// <remarks>
/// Only call from the build engine.
/// Do NOT use directly.  Instead, go through the build engine when doing builds as this behavior may be overriden by France or Habsburg
/// </remarks>
public class BuildOnceOnSpace_Default { 

	/// <returns>HumanToken or null.</returns>
	public async Task<HumanToken?> ActAsync( Space space ) {
		_space = space;
		await using var actionScope = await ActionScope.Start(ActionCategory.Invader);

		// Determine type to build
		var (countToAdd, invaderToAdd) = DetermineWhatToAdd();
		BuildEngine.InvaderToAdd.Value = invaderToAdd;

		// Check for Stoppers
		var buildStoppers = _space.ModsOfType<ISkipBuilds>()
			.OrderBy( t => t.Cost ) // cheapest first
			.ToArray();
		foreach(ISkipBuilds stopper in buildStoppers)
			if(await stopper.Skip( _space )){
				ActionScope.Current.Log( new Log.InvaderActionEntry( $"{_space.SpaceSpec.Label}: build stopped by {stopper.Text}" ) );
				return null;// "build stopped by " + stopper.Text;
			}

		// build it
		var added = await _space.AddDefaultAsync( invaderToAdd, countToAdd, AddReason.Build );
		ActionScope.Current.Log( new Log.InvaderActionEntry( $"{_space.SpaceSpec.Label}: {added.Added.Class.Label}" ) );
		return added.Added.AsHuman();
	}

	protected virtual (int,HumanTokenClass) DetermineWhatToAdd() {
		int townCount = _space!.Sum( Human.Town );
		int cityCount = _space!.Sum( Human.City );
		HumanTokenClass invaderToAdd = cityCount < townCount ? Human.City : Human.Town;
		int countToAdd = 1;
		return (countToAdd, invaderToAdd);
	}

	protected Space? _space;

}