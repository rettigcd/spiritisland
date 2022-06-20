namespace SpiritIsland;

public class BuildEngine {

	// Initialized at begining of Exec
	protected TokenCountDictionary tokens;
	protected GameState gameState;

	// Initialized at beginning of DoSingleBuildOnSpace
	protected Guid actionId;

	/// <summary>
	/// Call this on spaces, even if they have a disease.
	/// Does the remove-disease action instead of the build when necessary.
	/// </summary>
	/// <remarks>Not thread-safe / reentrant.  Can only call .Exec 1 at a time.</remarks>
	public async Task<string> Exec( TokenCountDictionary tokens, GameState gameState  ) {
		this.tokens = tokens;
		this.gameState = gameState;

		var results = new List<string>();

		// If multiple builds on same space, share the same ActionID
		int buildCounts = PullBuildTokens();
		while(buildCounts-- > 0)
			results.Add( await DoSingleBuildOnSpace() );

		return string.Join(", ",results);
	}

	int PullBuildTokens() {
		int buildCounts = tokens[TokenType.DoBuild];
		tokens.Init( TokenType.DoBuild, 0 );
		return buildCounts;
	}

	async Task<string> DoSingleBuildOnSpace() {
		actionId = Guid.NewGuid();

		// Determine type to build
		int townCount = tokens.Sum( Invader.Town );
		int cityCount = tokens.Sum( Invader.City );
		HealthTokenClass invaderToAdd = townCount > cityCount ? Invader.City : Invader.Town;

		var buildStoppers = tokens.Keys.OfType<IBuildStopper>()
			.Where( x => x.Stops( invaderToAdd ) )
			.Where( t => t != TokenType.Disease ) // handle disease separtely
			.ToArray();

		// !!! user should be able to select which non-disease stopper they want to use (if multiple)
		if(buildStoppers.Length > 0) {
			var stopper = buildStoppers[0];
			await stopper.StopBuild( gameState, tokens.Space );
			return "build stopped by " + stopper.Text;
		} 
		
		if(await StopBuildWithDiseaseBehavior())
			return "build stopped by disease";

		// build it
		await tokens.AddDefault( invaderToAdd, 1, actionId, AddReason.Build );
		return invaderToAdd.Label;

	}

	/// <remarks>
	/// This is inside of the Engine so that Vengenance can override it
	/// and allow a build even with the disase.
	/// </remarks>
	virtual protected async Task<bool> StopBuildWithDiseaseBehavior() {
		var disease = tokens.Disease;
		bool stoppedByDisease = disease.Any;
		if(stoppedByDisease)
			await disease.Bind(actionId).Remove(1, RemoveReason.UsedUp);

		return stoppedByDisease;
	}

}
