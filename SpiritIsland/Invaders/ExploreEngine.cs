namespace SpiritIsland;

public class ExploreEngine {

	public virtual async Task ActivateCard( InvaderCard card, GameState gameState ) {
		gameState.Log( new Log.InvaderActionEntry( "Exploring:" + card.Text ) );
		SpaceState[] tokenSpacesToExplore = PreExplore( card, gameState );
		await DoExplore( gameState, tokenSpacesToExplore, card.HasEscalation );

		if( card.HasEscalation && Escalation != null ) {
			await Escalation(gameState);
			card.HasEscalation = false;
		}
	}

	public Func<GameState, Task> Escalation;

	static protected SpaceState[] PreExplore( InvaderCard card, GameState gs ) {

		// Modify
		static bool IsExplorerSource( SpaceState space ) => space.Space.IsOcean || space.HasAny( Human.Town_City );

		var sources = gs.Spaces_Existing
			.Where( IsExplorerSource )
			.Where( ss => !ss.Keys.OfType<ISkipExploreFrom>().Any() )
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
			x.Adjust( ModToken.DoExplore, x.Space.Board.InvaderActionCount );

		return gs.Spaces
			.Where( x => x[ModToken.DoExplore] > 0 )
			.ToArray();
	}

	protected async Task DoExplore( GameState gs, SpaceState[] tokenSpacesToExplore, bool escalation ) {
		foreach(var exploreTokens in tokenSpacesToExplore) {
			int exploreCount = exploreTokens[ModToken.DoExplore];
			exploreTokens.Init( ModToken.DoExplore, 0 );
			while(0 < exploreCount--) {
				await using ActionScope actionScope = new ActionScope( ActionCategory.Invader );
				await ExploreSingleSpace( exploreTokens, gs, escalation );
			}
		}
	}

	protected virtual async Task ExploreSingleSpace( SpaceState tokens, GameState gs, bool escalation ) {
		var ctx = new GameCtx( gs );
		foreach(var stopper in tokens.Keys.OfType<ISkipExploreTo>().ToArray())
			if(await stopper.Skip( ctx, tokens ))
				return;

		gs.Log( new Log.SpaceExplored( tokens.Space ) );
		await AddToken( tokens );
	}

	protected virtual async Task AddToken( SpaceState tokens ) 
		=> await tokens.AddDefault( Human.Explorer, 1, AddReason.Explore );
}
