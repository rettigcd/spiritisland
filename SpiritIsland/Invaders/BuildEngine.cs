namespace SpiritIsland;

public class BuildEngine {

	public virtual async Task ActivateCard( InvaderCard card, GameState gameState ) {
		ActionScope.Current.Log( new Log.InvaderActionEntry( "Building:" + card.Text ) );
		AddBuildTokensMatchingCard( card, gameState );
		await Build( gameState );
	}

	static protected SpaceState[] GetSpacesMatchingCard( InvaderCard card, GameState gameState )
		=> gameState.Spaces.Where( card.MatchesCard ).ToArray();

	void AddBuildTokensMatchingCard( InvaderCard card, GameState gameState ) {
		var cardDependentBuildSpaces = GetSpacesMatchingCard( card, gameState );
		var spacesMatchingCardCriteria = cardDependentBuildSpaces
			.Where( ShouldBuildOnSpace )    // usually because it has invaders on it
			.ToArray();
		foreach(SpaceState tokens in spacesMatchingCardCriteria)
			tokens.Adjust( ModToken.DoBuild, tokens.Space.InvaderActionCount );

		// log any spaces that look like they should get built on but didn't
		var noBuildsSpaceNames = cardDependentBuildSpaces   // Space that should be build on
			.Except( spacesMatchingCardCriteria )    // Spaces that we are actually building on.
			.SelectLabels()
			.ToArray();

		if(0 < noBuildsSpaceNames.Length)
			ActionScope.Current.Log( new Log.InvaderActionEntry( "No build due to no invaders on: " + string.Join( ", ", noBuildsSpaceNames ) ) );
	}

	async Task Build( GameState gameState ) {

		// Scan for all Build Tokens - both Card-Build-Spaces plus any pre-existing DoBuilds
		// ** May contain more than just Normal Build, due to rule/power that added extra ones.
		var matchingSpacesWithBuildTokens = gameState.Spaces
			.Where( tokens => 0 < tokens[ModToken.DoBuild] )
			.OrderBy( tokens => tokens.Space.Label )
			.ToArray();

		// Do Builds on each space
		foreach(var spaceState in matchingSpacesWithBuildTokens)
			await DoAllBuildsInSpace( gameState, spaceState );

	}

	protected virtual async Task DoAllBuildsInSpace( GameState gameState, SpaceState spaceState ) {
		int buildCounts = PullBuildTokens( spaceState );
		while(0 < buildCounts--)
			await Do1Build( gameState, spaceState );
	}

	static int PullBuildTokens( SpaceState tokens ) {
		int buildCounts = tokens[ModToken.DoBuild];
		tokens.Init( ModToken.DoBuild, 0 );
		return buildCounts;
	}

	public virtual async Task Do1Build( GameState _, SpaceState spaceState ){
		var builtToken = await new BuildOnceOnSpace_Default().ActAsync( spaceState );
		if(builtToken is not null && BuildComplete is not null)
			await BuildComplete(spaceState,builtToken);
	}

	public event Func<SpaceState,HumanToken,Task> BuildComplete;

	public virtual bool ShouldBuildOnSpace(SpaceState spaceState ) => spaceState.HasInvaders();

	public static readonly ActionScopeValue<HumanTokenClass> InvaderToAdd = new( "InvaderToBuild", (HumanTokenClass)null );

}
