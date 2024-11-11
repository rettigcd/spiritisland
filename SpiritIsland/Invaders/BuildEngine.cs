namespace SpiritIsland;

public class BuildEngine {

	public virtual async Task ActivateCard( InvaderCard card, GameState gameState ) {
		ActionScope.Current.Log( new Log.InvaderActionEntry( "Building:" + card.Code ) );
		AddBuildTokensMatchingCard( card, gameState );
		await Build( gameState );
	}

	static protected Space[] GetSpacesMatchingCard( InvaderCard card, GameState _ )
		=> ActionScope.Current.Spaces.Where( card.MatchesCard ).ToArray();

	// step 1 - Add Build Tokens in correct spot.
	void AddBuildTokensMatchingCard( InvaderCard card, GameState gameState ) {
		var cardDependentBuildSpaces = GetSpacesMatchingCard( card, gameState );
		var spacesMatchingCardCriteria = cardDependentBuildSpaces
			.Where( ShouldBuildOnSpace )    // usually because it has invaders on it
			.ToArray();
		foreach(Space space in spacesMatchingCardCriteria)
			space.Adjust( ModToken.DoBuild, space.SpaceSpec.InvaderActionCount );

		// log any spaces that look like they should get built on but didn't
		var noBuildsSpaceNames = cardDependentBuildSpaces   // Space that should be build on
			.Except( spacesMatchingCardCriteria )    // Spaces that we are actually building on.
			.SelectLabels()
			.ToArray();

		if(0 < noBuildsSpaceNames.Length)
			ActionScope.Current.Log( new Log.InvaderActionEntry( "No build due to no invaders on: " + string.Join( ", ", noBuildsSpaceNames ) ) );
	}

	// step 2 - Build where there are build tokens.
	async Task Build( GameState gameState ) {

		// Scan for all Build Tokens - both Card-Build-Spaces plus any pre-existing DoBuilds
		// ** May contain more than just Normal Build, due to rule/power that added extra ones.
		var matchingSpacesWithBuildTokens = ActionScope.Current.Spaces
			.Where( tokens => 0 < tokens[ModToken.DoBuild] )
			.OrderBy( tokens => tokens.SpaceSpec.Label )
			.ToArray();

		// Do Builds on each space
		foreach(var space in matchingSpacesWithBuildTokens)
			await DoAllBuildsInSpace( gameState, space );

	}

	protected virtual async Task DoAllBuildsInSpace( GameState gameState, Space space ) {
		int buildCounts = PullBuildTokens( space );
		while(0 < buildCounts--)
			await TryToDo1Build( gameState, space );
	}

	/// <summary> Gets the Build tokens, then clears them. </summary>
	static int PullBuildTokens( Space space ) {
		int buildCounts = space[ModToken.DoBuild];
		space.Init( ModToken.DoBuild, 0 );
		return buildCounts;
	}

	/// <summary> Makes 1 attempt to build, may be stopped by Build-Stoppers </summary>
	public virtual async Task TryToDo1Build( GameState _, Space space ){
		var builtToken = await new BuildOnceOnSpace_Default().ActAsync( space );
		if(builtToken is not null && BuildComplete is not null)
			await BuildComplete(space,builtToken); // Trigger event
	}

	public event Func<Space,HumanToken,Task> BuildComplete;

	public virtual bool ShouldBuildOnSpace(Space space ) => space.HasInvaders();

	/// <summary>
	/// Makes Invader-to-Build visible to the current ActionScope
	/// </summary>
	static public readonly ActionScopeValue<HumanTokenClass> InvaderToAdd = new( "InvaderToBuild", (HumanTokenClass)null );

}
