namespace SpiritIsland.Basegame.Adversaries;

public class FranceExplorer : ExploreEngine {
	readonly bool _hasFrontierExploration;
	readonly bool _hasPersistentExplorers;


	public FranceExplorer( bool hasFrontierExploration, bool hasPersistentExplorers ) {
		_hasFrontierExploration = hasFrontierExploration;
		_hasPersistentExplorers = hasPersistentExplorers;
	}	

	public override async Task ActivateCard( InvaderCard card, GameState gameState ) { 
		await base.ActivateCard( card, gameState );

		// Original
		SpaceState[] tokenSpacesToExplore = PreExplore( card, gameState );
		await DoExplore( gameState, tokenSpacesToExplore, false );

		await using var scope = await ActionScope.Start(ActionCategory.Adversary);

		if(_hasFrontierExploration)
			await DoFrontierExploration( gameState, tokenSpacesToExplore );

		if(card.HasEscalation) {
			await DemandForNewCashCrops( gameState, card );
			card.HasEscalation = false;
		}

		if(_hasPersistentExplorers)
			await PersistentExplorers( gameState );

	}

	static Task PersistentExplorers( GameState GameState ) {
		// After resolving an Explore Card, on each board add 1 Explorer to a land without any. 
		return Cmd.ForEachBoard( AddExplorerToLandWithoutAny ).ActAsync( GameState );

	}

	static BaseCmd<BoardCtx> AddExplorerToLandWithoutAny => new BaseCmd<BoardCtx>(
		"Add Explorer to Land without any",
		async boardCtx => {
			var options = boardCtx.Board.Spaces.Where( s => !s.Tokens.HasAny( Human.Explorer ) ).ToArray();
			var space = await boardCtx.SelectAsync( new A.Space( "Add explorer", options, Present.Always ) );
			if(space != null)
				await space.Tokens.AddDefault( Human.Explorer, 1 );
		}
	);


	async Task DoFrontierExploration( GameState gs, SpaceState[] tokenSpacesToExplore ) {
		// Frontier Explorers: Except during Setup: After Invaders successfully Explore into a land which had no Town / City, add 1 Explorer there.
		foreach(var exploreTokens in tokenSpacesToExplore)
			if(!exploreTokens.HasAny( Human.Town_City )) {
				await using var scope = await ActionScope.Start(ActionCategory.Adversary);
				await ExploreSingleSpace( exploreTokens, gs, false );
			}
	}

	static Task DemandForNewCashCrops( GameState gs, InvaderCard card ) {
		// Demand for New Cash Crops:
		// After Exploring, on each board, pick a land of the shown terrain.If it has Town / City, add 1 Blight.Otherwise, add 1 Town

		static SpiritAction SelectSpaceAction( SpaceState s ) {
			return s.HasAny( Human.Town_City )
				? new SpiritAction( "Add 1 Town to " + s.Space.Text, null ) // !!! implement
				: new SpiritAction( "", null ); // !!! implement
		}

		return Cmd.ForEachBoard( new BaseCmd<BoardCtx>(
			"Place town or blight matching Explore card."
			, boardCtx => Cmd.Pick1( 
				boardCtx.Board.Spaces.Tokens()
					.Where( card.MatchesCard )
					.Select( SelectSpaceAction )
					.ToArray()
			).ActAsync( boardCtx.Self )
		) ).ActAsync( gs );

	}


}