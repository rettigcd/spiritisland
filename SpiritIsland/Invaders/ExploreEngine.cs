namespace SpiritIsland;

public class ExploreEngine {

	public Func<GameState, Task> Escalation;
	public event Func<GameState,Task> ExploreForCardComplete;
	public event Func<Space,Task> ExploredSpace;

	public virtual async Task ActivateCard( InvaderCard card, GameState gameState ) {
		ActionScope.Current.Log( new Log.InvaderActionEntry( "Exploring:" + card.Code ) );
		Space[] tokenSpacesToExplore = PreExplore( card );
		await ExplorePerMarker_ManySpaces_Stoppable( tokenSpacesToExplore, card.TriggersEscalation );

		if( card.TriggersEscalation && Escalation != null )
			await Escalation(gameState);

		if(ExploreForCardComplete is not null)
			await ExploreForCardComplete(gameState);
	}

	static protected Space[] PreExplore( InvaderCard card ) {

		// Modify
		static bool IsExplorerSource( Space space ) => space.SpaceSpec.IsOcean || space.HasAny( Human.Town_City );

		var sources = ActionScope.Current.Spaces_Existing
			.Where( IsExplorerSource )
			.Where( ss => !ss.ModsOfType<ISkipExploreFrom>().Any() )
			.ToHashSet();

		var exploreRoutes = sources.SelectMany(
			source=>source.Adjacent_ForInvaders.Union(new []{source})
				.Where( card.MatchesCard )
				.Select(dst => new ExploreRoute { Source = source, Destination = dst })
			)
			.OrderBy( route => route.Destination.SpaceSpec.Label )
			.ThenBy( route => route.Source.SpaceSpec.Label )
			.ToArray();

		var spacesWeExplore = exploreRoutes
			.Where( rt => rt.IsValid )
			.Select( rt => rt.Destination )
			.Distinct()
			.OrderBy( x => x.SpaceSpec.Label )
			.ToArray();


		foreach(var x in spacesWeExplore)
			x.Adjust(InvaderActionToken.DoExplore, x.SpaceSpec.InvaderActionCount );

		return ActionScope.Current.Spaces
			.Where( x => x[InvaderActionToken.DoExplore] > 0 )
			.ToArray();
	}

	protected async Task ExplorePerMarker_ManySpaces_Stoppable( Space[] tokenSpacesToExplore, bool escalation ) {
		foreach(var exploredSpace in tokenSpacesToExplore)
			await ExplorePerMarker_1Space_Stoppable( escalation, exploredSpace );
	}

	async Task ExplorePerMarker_1Space_Stoppable( bool escalation, Space exploredSpace ) {
		int markerCount = exploredSpace[InvaderActionToken.DoExplore];
		exploredSpace.Init(InvaderActionToken.DoExplore, 0 );
		while(0 < markerCount--) {
			await using ActionScope actionScope = await ActionScope.Start( ActionCategory.Invader );
			await Explore_1Space_Stoppable( exploredSpace, escalation );
		}
	}

	protected virtual async Task Explore_1Space_Stoppable( Space space, bool escalation ) {

		foreach(ISkipExploreTo stopper in space.ModsOfType<ISkipExploreTo>().ToArray())
			if(await stopper.Skip( space ))
				return;

		ActionScope.Current.Log( new Log.SpaceExplored( space.SpaceSpec ) );
		await AddToken( space );
		if(ExploredSpace is not null) 
			await ExploredSpace( space );
	}

	protected virtual async Task AddToken( Space space ) 
		=> await space.AddDefaultAsync( Human.Explorer, 1, AddReason.Explore );
}
