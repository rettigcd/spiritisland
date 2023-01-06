namespace SpiritIsland;

public class ExploreEngine {

	public virtual async Task ActivateCard( IInvaderCard card, GameState gameState ) {
		gameState.Log( new InvaderActionEntry( "Exploring:" + card.Text ) );
		SpaceState[] tokenSpacesToExplore = PreExplore( card, gameState );
		await DoExplore( gameState, tokenSpacesToExplore, card.HasEscalation );

		if( card.HasEscalation && Escalation != null ) {
			await Escalation(gameState);
			card.HasEscalation = false;
		}
	}

	public Func<GameState, Task> Escalation;

	static protected SpaceState[] PreExplore( IInvaderCard card, GameState gs ) {

		// Modify
		static bool IsExplorerSource( SpaceState space ) => space.Space.IsOcean || space.HasAny( Invader.Town, Invader.City );

		var sources = gs.AllActiveSpaces
			.Where( IsExplorerSource )
			.Where( ss => !ss.Keys.OfType<ISkipExploreFrom>().Any() )
			.ToHashSet();


		var exploreRoutes = gs.AllActiveSpaces.Where( card.MatchesCard )
			.SelectMany( dst => dst.Range( 1 )
				.Where( sources.Contains )
				.Select( src => new ExploreRoute { Source = src, Destination = dst } )
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
			x.Adjust( TokenType.DoExplore, 1 );

		return gs.AllActiveSpaces
			.Where( x => x[TokenType.DoExplore] > 0 )
			.ToArray();
	}

	protected async Task DoExplore( GameState gs, SpaceState[] tokenSpacesToExplore, bool escalation ) {
		foreach(var exploreTokens in tokenSpacesToExplore) {
			int exploreCount = exploreTokens[TokenType.DoExplore];
			exploreTokens.Init( TokenType.DoExplore, 0 );
			while(0 < exploreCount--)
				using(UnitOfWork actionScope = gs.StartAction( ActionCategory.Invader ))
					await ExploreSingleSpace( exploreTokens, gs, actionScope, escalation );
		}
	}

	protected virtual async Task ExploreSingleSpace( SpaceState tokens, GameState gs, UnitOfWork actionScope, bool escalation ) {
		var ctx = new GameCtx( gs, actionScope );
		foreach(var stopper in tokens.Keys.OfType<ISkipExploreTo>().ToArray())
			if(await stopper.Skip( ctx, tokens ))
				return;

		gs.Log( new SpaceExplored( tokens.Space ) );
		await tokens.AddDefault( Invader.Explorer, 1, actionScope, AddReason.Explore );
	}

}
