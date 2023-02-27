namespace SpiritIsland;

/// <summary> Performs 1 builds on 1 space </summary>
public class BuildOnceOnSpace {

	public BuildOnceOnSpace( GameState gs, SpaceState tokens ) {
		_gameState = gs;
		_tokens = tokens;
	}

	public async Task Exec() {
		string buildResult = await GetResult();
		_gameState.Log( new Log.InvaderActionEntry( _tokens.Space.Label + ": " + buildResult ) );
	}

	async Task<string> GetResult() {

		await using var actionScope = new ActionScope( ActionCategory.Invader );

		// Determine type to build
		var (countToAdd, invaderToAdd) = DetermineWhatToAdd();
		BuildEngine.InvaderToAdd.Value = invaderToAdd;


		var buildStoppers = _tokens.ModsOfType<ISkipBuilds>()
			.OrderBy( t => t.Cost ) // cheapest first
			.ToArray();

		// Check for Stoppers
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

	readonly protected SpaceState _tokens;
	readonly protected GameState _gameState;

}