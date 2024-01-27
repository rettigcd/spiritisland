namespace SpiritIsland;

public class ExploreEngine {

	public Func<GameState, Task> Escalation;
	public event Func<GameState,Task> ExplorePhaseComplete;
	public event Func<SpaceState,Task> ExploredSpace;

	public virtual async Task ActivateCard( InvaderCard card, GameState gameState ) {
		ActionScope.Current.Log( new Log.InvaderActionEntry( "Exploring:" + card.Text ) );
		SpaceState[] tokenSpacesToExplore = PreExplore( card, gameState );
		await ExplorePerMarker_ManySpaces_Stoppable( gameState, tokenSpacesToExplore, card.TriggersEscalation );

		if( card.TriggersEscalation && Escalation != null )
			await Escalation(gameState);

		if(ExplorePhaseComplete is not null)
			await ExplorePhaseComplete(gameState);
	}

	static protected SpaceState[] PreExplore( InvaderCard card, GameState gs ) {

		// Modify
		static bool IsExplorerSource( SpaceState space ) => space.Space.IsOcean || space.HasAny( Human.Town_City );

		var sources = gs.Spaces_Existing
			.Where( IsExplorerSource )
			.Where( ss => !ss.ModsOfType<ISkipExploreFrom>().Any() )
			.ToHashSet();

		var exploreRoutes = sources.SelectMany(
			source=>source.Adjacent_ForInvaders.Union(new []{source})
				.Where( card.MatchesCard )
				.Select(dst => new ExploreRoute { Source = source, Destination = dst })
			)
			.OrderBy( route => route.Destination.Space.Label )
			.ThenBy( route => route.Source.Space.Label )
			.ToArray();

		var spacesWeExplore = exploreRoutes
			.Where( rt => rt.IsValid )
			.Select( rt => rt.Destination )
			.Distinct()
			.OrderBy( x => x.Space.Label )
			.ToArray();


		foreach(var x in spacesWeExplore)
			x.Adjust( ModToken.DoExplore, x.Space.InvaderActionCount );

		return gs.Spaces
			.Where( x => x[ModToken.DoExplore] > 0 )
			.ToArray();
	}

	protected async Task ExplorePerMarker_ManySpaces_Stoppable( GameState gs, SpaceState[] tokenSpacesToExplore, bool escalation ) {
		foreach(var exploredSpaceState in tokenSpacesToExplore)
			await ExplorePerMarker_1Space_Stoppable( gs, escalation, exploredSpaceState );
	}

	async Task ExplorePerMarker_1Space_Stoppable( GameState gs, bool escalation, SpaceState exploredSpaceState ) {
		int markerCount = exploredSpaceState[ModToken.DoExplore];
		exploredSpaceState.Init( ModToken.DoExplore, 0 );
		while(0 < markerCount--) {
			await using ActionScope actionScope = await ActionScope.Start( ActionCategory.Invader );
			await Explore_1Space_Stoppable( exploredSpaceState, gs, escalation );
		}
	}

	protected virtual async Task Explore_1Space_Stoppable( SpaceState tokens, GameState gs, bool escalation ) {

		foreach(ISkipExploreTo stopper in tokens.ModsOfType<ISkipExploreTo>().ToArray())
			if(await stopper.Skip( tokens ))
				return;

		ActionScope.Current.Log( new Log.SpaceExplored( tokens.Space ) );
		await AddToken( tokens );
		if(ExploredSpace is not null) 
			await ExploredSpace( tokens );
	}

	protected virtual async Task AddToken( SpaceState tokens ) 
		=> await tokens.AddDefaultAsync( Human.Explorer, 1, AddReason.Explore );
}
