namespace SpiritIsland.Basegame.Adversaries;

public class FranceExplorer	: ExploreEngine {

	public FranceExplorer( bool hasFrontierExploration, bool hasPersistentExplorers ){
		_hasFrontierExploration = hasFrontierExploration;
		_hasPersistentExplorers = hasPersistentExplorers;

		Escalation = France.DemandForNewCashCrops;
	}
	readonly bool _hasFrontierExploration;
	readonly bool _hasPersistentExplorers;


	public override async Task ActivateCard( InvaderCard card, GameState gameState ) { 
		await base.ActivateCard( card, gameState );

		// Original
		SpaceState[] tokenSpacesToExplore = PreExplore( card, gameState );
		await ExplorePerMarker_ManySpaces_Stoppable( gameState, tokenSpacesToExplore, false );

		await using var scope = await ActionScope.Start(ActionCategory.Adversary);

		if(_hasFrontierExploration)
			await DoFrontierExploration( gameState, tokenSpacesToExplore );

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
				await space.Tokens.AddDefaultAsync( Human.Explorer, 1 );
		}
	);


	async Task DoFrontierExploration( GameState gs, SpaceState[] tokenSpacesToExplore ) {
		// Frontier Explorers: 
		// Except during Setup: 
		// After Invaders successfully Explore into a land which had no Town / City, add 1 Explorer there.
		foreach(var exploreTokens in tokenSpacesToExplore)
			if(!exploreTokens.HasAny( Human.Town_City )) {
				await using var scope = await ActionScope.Start(ActionCategory.Adversary);
				await Explore_1Space_Stoppable( exploreTokens, gs, false );
			}
	}

}