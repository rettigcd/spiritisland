namespace SpiritIsland;

public class BuildEngine {

	public virtual async Task ActivateCard( IInvaderCard card, GameState gameState ) {
		gameState.Log( new InvaderActionEntry( "Building:" + card.Text ) );
		AddBuildTokensMatchingCard( card, gameState );
		await Build( gameState );
	}

	void AddBuildTokensMatchingCard( IInvaderCard card, GameState gameState ) {
		var cardDependentBuildSpaces = gameState.AllActiveSpaces
			.Where( card.MatchesCard )      // space matches card
			.ToArray();
		var spacesMatchingCardCriteria = cardDependentBuildSpaces
			.Where( ShouldBuildOnSpace )    // usually because it has invaders on it
			.ToArray();
		foreach(SpaceState tokens in spacesMatchingCardCriteria)
			tokens.Adjust( TokenType.DoBuild, 1 );

		// log any spaces that look like they should get built on but didn't
		var noBuildsSpaceNames = cardDependentBuildSpaces   // Space that should be build on
			.Except( spacesMatchingCardCriteria )    // Spaces that we are actually building on.
			.Select( x => x.Space.Text )
			.ToArray();

		if(0 < noBuildsSpaceNames.Length)
			gameState.Log( new InvaderActionEntry( "No build due to no invaders on: " + string.Join( ", ", noBuildsSpaceNames ) ) );
	}

	async Task Build( GameState gameState ) {

		// Scan for all Build Tokens - both Card-Build-Spaces plus any pre-existing DoBuilds
		// ** May contain more than just Normal Build, due to rule/power that added extra ones.
		var matchingSpacesWithBuildTokens = gameState.AllActiveSpaces
			.Where( tokens => 0 < tokens[TokenType.DoBuild] )
			.OrderBy( tokens => tokens.Space.Label )
			.ToArray();

		// Do Builds on each space
		foreach(var spaceState in matchingSpacesWithBuildTokens)
			await DoAllBuildsInSpace( gameState, spaceState );

	}

	async Task DoAllBuildsInSpace( GameState gameState, SpaceState spaceState ) {
		int buildCounts = PullBuildTokens( spaceState );
		while(0 < buildCounts--)
			await Do1Build( gameState, spaceState );
	}

	static int PullBuildTokens( SpaceState tokens ) {
		int buildCounts = tokens[TokenType.DoBuild];
		tokens.Init( TokenType.DoBuild, 0 );
		return buildCounts;
	}

	public virtual Task Do1Build( GameState gameState, SpaceState spaceState ) 
		=> new BuildOnceOnSpace( gameState, spaceState ).Exec();

	public virtual bool ShouldBuildOnSpace(SpaceState spaceState ) => spaceState.HasInvaders();

}

/// <summary> Performs 1 builds on 1 space </summary>
public sealed class BuildOnceOnSpace {

	public BuildOnceOnSpace( GameState gs, SpaceState tokens ) {
		_gameState = gs;
		_tokens = tokens;
	}

	public async Task Exec() {
		string buildResult = await GetResult();
		_gameState.Log( new InvaderActionEntry( _tokens.Space.Label + ": " + buildResult ) );
	}

	async Task<string> GetResult() {

		await using var uow = _gameState.StartAction( ActionCategory.Invader );

		// Determine type to build
		int townCount = _tokens.Sum( Invader.Town );
		int cityCount = _tokens.Sum( Invader.City );
		HealthTokenClass invaderToAdd = cityCount < townCount ? Invader.City : Invader.Town;

		var buildStoppers = _tokens.Keys.OfType<ISkipBuilds>()
			.OrderBy( t => t.Cost ) // cheapest first
			.ToArray();

		// Check for Stoppers
		var gameCtx = new GameCtx( _gameState, uow );
		foreach(var stopper in buildStoppers)
			if(await stopper.Skip( gameCtx, _tokens, invaderToAdd ))
				return "build stopped by " + stopper.Text;

		// build it
		await _tokens.AddDefault( invaderToAdd, 1, gameCtx.UnitOfWork, AddReason.Build );
		return invaderToAdd.Label;
	}


	readonly SpaceState _tokens;
	readonly GameState _gameState;

}