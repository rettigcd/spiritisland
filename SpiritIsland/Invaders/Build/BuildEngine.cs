namespace SpiritIsland;

/// <summary> Performs all builds on 1 space </summary>
public sealed class BuildEngine {

	public BuildEngine(GameState gs,SpaceState tokens) {
		gameState = gs;
		this.tokens = tokens;
	}

	// Initialized at begining of Exec
	readonly SpaceState tokens;
	readonly GameState gameState;

	/// <summary>
	/// Call this on spaces, even if they have a disease.
	/// Does the remove-disease action instead of the build when necessary.
	/// </summary>
	/// <remarks>Not thread-safe / reentrant.  Can only call .Exec 1 at a time.</remarks>
	public async Task<string> DoBuilds() {
		var results = new List<string>();

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
		await using var uow = gameState.StartAction( ActionCategory.Invader );

		// Determine type to build
		int townCount = tokens.Sum( Invader.Town );
		int cityCount = tokens.Sum( Invader.City );
		HealthTokenClass invaderToAdd = cityCount < townCount  ? Invader.City : Invader.Town;

		var buildStoppers = tokens.Keys.OfType<ISkipBuilds>()
			.OrderBy(t=>t.Cost) // cheapest first
			.ToArray();

		// Check for Stoppers
		var gameCtx = new GameCtx( gameState, uow );
		foreach(var stopper in buildStoppers )
			if( await stopper.Skip( gameCtx, tokens, invaderToAdd ))
				return "build stopped by " + stopper.Text;
		
		// build it
		await tokens.AddDefault( invaderToAdd, 1, gameCtx.UnitOfWork, AddReason.Build );
		return invaderToAdd.Label;

	}

}
