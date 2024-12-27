namespace SpiritIsland;

public class BuildEngine {

	public event Func<Space, HumanToken, Task>? BuildComplete;

	public virtual async Task ActivateCard( InvaderCard card ) {
		ActionScope.Current.Log( new Log.InvaderActionEntry( "Building:" + card.Code ) );
		AddBuildTokensMatchingCard( card );
		await Build();
	}

	static protected Space[] GetSpacesMatchingCard( InvaderCard card )
		=> ActionScope.Current.Spaces.Where( card.MatchesCard ).ToArray();

	// step 1 - Add Build Tokens in correct spot.
	void AddBuildTokensMatchingCard( InvaderCard card ) {
		var cardDependentBuildSpaces = GetSpacesMatchingCard( card );
		var spacesMatchingCardCriteria = cardDependentBuildSpaces
			.Where( ShouldBuildOnSpace )    // usually because it has invaders on it
			.ToArray();
		foreach(Space space in spacesMatchingCardCriteria)
			space.Adjust(InvaderActionToken.DoBuild, space.SpaceSpec.InvaderActionCount );

		// log any spaces that look like they should get built on but didn't
		var noBuildsSpaceNames = cardDependentBuildSpaces   // Space that should be build on
			.Except( spacesMatchingCardCriteria )    // Spaces that we are actually building on.
			.SelectLabels()
			.ToArray();

		if(0 < noBuildsSpaceNames.Length)
			ActionScope.Current.Log( new Log.InvaderActionEntry( "No build due to no invaders on: " + string.Join( ", ", noBuildsSpaceNames ) ) );
	}

	// step 2 - Build where there are build tokens.
	async Task Build() {

		// Scan for all Build Tokens - both Card-Build-Spaces plus any pre-existing DoBuilds
		// ** May contain more than just Normal Build, due to rule/power that added extra ones.
		var matchingSpacesWithBuildTokens = ActionScope.Current.Spaces
			.Where( tokens => 0 < tokens[InvaderActionToken.DoBuild] )
			.OrderBy( tokens => tokens.SpaceSpec.Label )
			.ToArray();

		// Do Builds on each space
		foreach(var space in matchingSpacesWithBuildTokens)
			await DoAllBuildsInSpace( space );

	}

	protected virtual async Task DoAllBuildsInSpace( Space space ) {
		int buildCounts = PullBuildTokens( space );
		while(0 < buildCounts--)
			await TryToDo1Build( space );
	}

	/// <summary> Gets the Build tokens, then clears them. </summary>
	static int PullBuildTokens( Space space ) {
		int buildCounts = space[InvaderActionToken.DoBuild];
		space.Init( InvaderActionToken.DoBuild, 0 );
		return buildCounts;
	}

	/// <summary> Makes 1 attempt to build, may be stopped by Build-Stoppers </summary>
	public async Task TryToDo1Build( Space space ){
		var builtToken = await OneSpacebuilder.ActAsync( space );
		// publish event
		if(builtToken is not null && BuildComplete is not null)
			await BuildComplete(space,builtToken); // Trigger event
	}

	public BuildOnceOnSpace_Default OneSpacebuilder = new BuildOnceOnSpace_Default();

	public virtual bool ShouldBuildOnSpace(Space space ) => space.HasInvaders();

	/// <summary>
	/// Makes Invader-to-Build visible to the current ActionScope
	/// </summary>
	static public readonly ActionScopeValueNullable<HumanTokenClass> InvaderToAdd = new( "InvaderToBuild" );

}
