namespace SpiritIsland.Basegame.Adversaries;

public class FranceExplorer : ExploreEngine {
	readonly bool hasFrontierExploration;
	readonly bool hasPersistentExplorers;
	public FranceExplorer( int level ) {
		hasFrontierExploration = 1 <= level;
		hasPersistentExplorers = 6 <= level;
	}

	public override async Task ActivateCard( InvaderCard card, GameState gameState ) { 
		await base.ActivateCard( card, gameState );

		// Original
		SpaceState[] tokenSpacesToExplore = PreExplore( card, gameState );
		await DoExplore( gameState, tokenSpacesToExplore, false );

		await using var scope = await ActionScope.Start(ActionCategory.Adversary);
		GameCtx gameCtx = new GameCtx( gameState );

		if(hasFrontierExploration)
			await DoFrontierExploration( gameState, tokenSpacesToExplore );

		if(card.HasEscalation) {
			await DemandForNewCashCrops( gameCtx, card );
			card.HasEscalation = false;
		}

		if(hasPersistentExplorers)
			await PersistentExplorers( gameCtx );

	}

	static Task PersistentExplorers( GameCtx gameCtx ) {
		// After resolving an Explore Card, on each board add 1 Explorer to a land without any. 
		return Cmd.ForEachBoard( AddExplorerToLandWithoutAny ).ActAsync( gameCtx );

	}

	static BaseCmd<BoardCtx> AddExplorerToLandWithoutAny => new BaseCmd<BoardCtx>(
		"Add Explorer to Land without any",
		async boardCtx => {
			var options = boardCtx.Board.Spaces.Where( s => !s.Tokens.HasAny( Human.Explorer ) ).ToArray();
			var space = await boardCtx.Decision( new Select.ASpace( "Add explorer", options, Present.Always ) );
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

	static Task DemandForNewCashCrops( GameCtx ctx, InvaderCard card ) {
		// Demand for New Cash Crops:
		// After Exploring, on each board, pick a land of the shown terrain.If it has Town / City, add 1 Blight.Otherwise, add 1 Town

		static SpiritAction SelectSpaceAction( SpaceState s ) {
			return s.HasAny( Human.Town_City )
				? new SpiritAction( "Add 1 Town to " + s.Space.Text, null )
				: new SpiritAction( "", null );
		}

		return Cmd.ForEachBoard( new BaseCmd<BoardCtx>(
			"Place town or blight matching Explore card."
			, boardCtx => Cmd.Pick1( boardCtx.Board.Spaces.Tokens()
				.Where( card.MatchesCard )
				.Select( SelectSpaceAction )
				.ToArray()
			).ActAsync( boardCtx )
		) ).ActAsync( ctx );

	}


}