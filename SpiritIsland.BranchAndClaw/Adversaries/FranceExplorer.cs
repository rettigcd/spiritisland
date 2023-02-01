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

		var gameCtx = new GameCtx( gameState, ActionCategory.Adversary ); // 1 action for all these things.

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
		return Cmd.ForEachBoard( AddExplorerToLandWithoutAny ).Execute( gameCtx );

	}

	static DecisionOption<BoardCtx> AddExplorerToLandWithoutAny => new DecisionOption<BoardCtx>(
		"Add Explorer to Land without any",
		async boardCtx => {
			var options = boardCtx.Board.Spaces.Where( s => !boardCtx.GameState.Tokens[s].HasAny( Human.Explorer ) ).ToArray();
			var space = await boardCtx.Decision( new Select.Space( "Add explorer", options, Present.Always ) );
			if(space != null)
				await boardCtx.GameState.Tokens[space].Bind( boardCtx.ActionScope ).AddDefault( Human.Explorer, 1 );
		}
	);


	async Task DoFrontierExploration( GameState gs, SpaceState[] tokenSpacesToExplore ) {
		// Frontier Explorers: Except during Setup: After Invaders successfully Explore into a land which had no Town / City, add 1 Explorer there.
		foreach(var exploreTokens in tokenSpacesToExplore)
			if(!exploreTokens.HasAny( Human.Town_City ))
				await ExploreSingleSpace( exploreTokens, gs, gs.StartAction( ActionCategory.Adversary ), false ); // !!! implemented as a new Action - Should it be???
	}

	static Task DemandForNewCashCrops( GameCtx ctx, InvaderCard card ) {
		// Demand for New Cash Crops:
		// After Exploring, on each board, pick a land of the shown terrain.If it has Town / City, add 1 Blight.Otherwise, add 1 Town

		static DecisionOption<SelfCtx> SelectSpaceAction( SpaceState s ) {
			return s.HasAny( Human.Town_City )
				? new DecisionOption<SelfCtx>( "Add 1 Town to " + s.Space.Text, null )
				: new DecisionOption<SelfCtx>( "", null );
		}

		return Cmd.ForEachBoard( new DecisionOption<BoardCtx>(
			"Place town or blight matching Explore card."
			, boardCtx => Cmd.Pick1( boardCtx.GameState.Tokens.PowerUp( boardCtx.Board.Spaces )
				.Where( card.MatchesCard )
				.Select( SelectSpaceAction )
				.ToArray()
			).Execute( boardCtx )
		) ).Execute( ctx );

	}


}