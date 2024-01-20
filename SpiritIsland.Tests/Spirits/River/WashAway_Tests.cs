namespace SpiritIsland.Tests.Spirits.River;

[Collection("BaseGame Spirits")]
public class WashAway_Tests {

	readonly Spirit _spirit;
	GameState _gameState;
	PowerCard _card;

	public WashAway_Tests():base( ) {
		_spirit = new RiverSurges();
	}

	[Fact]
	public async Task Nothing() {
		UserGateway.UsePreselect.Value = true;

		Given_RiverPlayingWashAway();
		_gameState.Phase = Phase.Slow;

		// no explorers

		// If not using pre-selects, will still target a space.

		// When_PlayingCard();
		await _card.ActivateAsync( _spirit )
			.AwaitUser(_spirit, 
				u=>u.NextDecision.HasPrompt("Wash Away: Target Space").Choose("A1")
			)
			.ShouldComplete();
	}

	[Trait("Feature","Push")]
	[Theory]
	[InlineData(1,0,0, "E@1", "",    "1E@1")]
	[InlineData(0,1,0, "T@2", "",    "1T@2")]
	[InlineData(1,0,1, "E@1", "1C@3","1E@1")]
	public async Task OneTarget1PushableType(
		int explorerCount, 
		int townCount, 
		int cityCount,
		string tokenToPush,
		string expectedTargetResult, 
		string expectedDestinationResult
	) {
		UserGateway.UsePreselect.Value = true;

		Given_RiverPlayingWashAway();
		_gameState.Phase = Phase.Slow;

		// 1 explorer on A4
		var board = _gameState.Island.Boards[0];
		Space targetSpace = board[4];
		var grp = _gameState.Tokens[targetSpace];
		grp.AdjustDefault( Human.Explorer, explorerCount );
		grp.AdjustDefault( Human.Town, townCount );
		grp.AdjustDefault( Human.City, cityCount );

		await _card.ActivateAsync( _spirit ).AwaitUser( _spirit, u => {
			u.NextDecision.HasPrompt("Wash Away: Target Space").Choose(targetSpace.Label);
			u.NextDecision.HasPrompt("Push up to (1)").MoveFrom(tokenToPush, tokenToPush + ",Done" )
				.MoveTo("A2","A1,A2,A3,A5");
		} ).ShouldComplete();

		// check that explore was moved
		targetSpace.Assert_HasInvaders( expectedTargetResult );
		board[2].Assert_HasInvaders( expectedDestinationResult );
	}

	// WashAway: Multiple target lands

	// WashAway: into ocean?
	[Trait("Feature","Push")]
	[Fact]
	public async Task DoesntPushIntoOcean(){

		Given_RiverPlayingWashAway( "A4" );
		_gameState.Phase = Phase.Slow;

		// 1 explorer on A2
		var board = _gameState.Island.Boards[0];
		Space targetSpace = board[2];
		_gameState.Tokens[targetSpace].AdjustDefault(Human.Explorer, 1);

		// When pushing explorer
		await _card.ActivateAsync( _spirit )
			.AwaitUser( _spirit, user => {
				user.NextDecision.HasPrompt("Wash Away: Target Space").Choose("A2");
				// Can't push into ocean
				user.NextDecision.MoveFrom("E@1","E@1,Done").MoveTo("A3","A1,A3,A4");
			} );

	}

	[Trait("Feature","Push")]
	[Fact]
	public async Task One1Target2PushableTypes() {
	
		Given_RiverPlayingWashAway();
		_gameState.Phase = Phase.Slow;

		// 1 explorer + 1 Town on A4
		var board = _gameState.Island.Boards[0];
		Space targetSpace = board[4];
		SpaceState grp =_gameState.Tokens[targetSpace];
		grp.AdjustDefault( Human.Explorer, 1 );
		grp.AdjustDefault( Human.Town, 1 );

		Space explorerDestination = board[2];
		Space townDestination = board[3];

		await _spirit.When_ResolvingCard<WashAway>( u => {
			u.NextDecision.HasPrompt( "Wash Away: Target Space").Choose("A4");
			u.NextDecision.HasPrompt( "Push up to (2)" ).MoveFrom("E@1","E@1,T@2,Done" ).MoveTo("A2","A1,A2,A3,A5");
			u.NextDecision.HasPrompt( "Push up to (1)" ).MoveFrom("T@2","T@2,Done" ).MoveTo("A3","A1,A2,A3,A5");
		} ).ShouldComplete();

		// check that explore was moved
		targetSpace.Assert_HasInvaders("");
		explorerDestination.Assert_HasInvaders( "1E@1" );
		townDestination.Assert_HasInvaders( "1T@2" );
	}

	[Trait("Feature","Push")]
	[Fact]
	public async Task DamagedTown() {
		UserGateway.UsePreselect.Value = true;

		Given_RiverPlayingWashAway();
		_gameState.Phase = Phase.Slow;

		// 1 damaged town on A4
		var board = _gameState.Island.Boards[0];
		Space targetSpace = board[4];
		_gameState.Tokens[targetSpace].Setup( StdTokens.Town1, 1 );

		var invaderDestination = board[2];

		await _card.ActivateAsync( _spirit ).AwaitUser(_spirit,user=>{ 
			user.NextDecision.HasPrompt( "Wash Away: Target Space").Choose("A4");
			user.NextDecision.HasPrompt( "Push up to (1)" ).MoveFrom("T@1","T@1,Done").MoveTo("A2","A1,A2,A3,A5");
		}).ShouldComplete();

		// check that explore was moved
		targetSpace.Assert_HasInvaders("");
		invaderDestination.Assert_HasInvaders( "1T@1" );
	}

	// WashAway: push 3 different invaders to 3 different lands
	// WashAway: multiple invader types
	[Trait("Feature","Push")]
	[Fact]
	public async Task Push3InvadersToDifferentLands() {
		Given_RiverPlayingWashAway();
		_gameState.Phase = Phase.Slow;

		// 31 explorers on A4
		var board = _gameState.Island.Boards[0];
		Space targetSpace = board[4];
		_gameState.Tokens[ targetSpace ].AdjustDefault( Human.Explorer, 3 );

		//  When: activating card
		await _card.ActivateAsync( _spirit ).AwaitUser( _spirit, user => { 
			user.NextDecision.HasPrompt( "Wash Away: Target Space").Choose("A4");
			user.NextDecision.HasPrompt("Push up to (3)").MoveFrom("E@1","E@1,Done").MoveTo("A2","A1,A2,A3,A5");
			user.NextDecision.HasPrompt("Push up to (2)").MoveFrom("E@1","E@1,Done").MoveTo("A3","A1,A2,A3,A5");
			user.NextDecision.HasPrompt("Push up to (1)").MoveFrom("E@1","E@1,Done").MoveTo("A5","A1,A2,A3,A5");
		} ).ShouldComplete();

		// check that explore was moved
		targetSpace.Assert_HasInvaders("");
		board[2].Assert_HasInvaders( "1E@1" );
		board[3].Assert_HasInvaders( "1E@1" );
		board[5].Assert_HasInvaders( "1E@1" );
	}

	void Given_RiverPlayingWashAway(string startingPresence="A5") {
		// A5 is the 'Y' land in the middle
		_gameState = new GameState(_spirit,Boards.A);

		//   And: a game on Board-A
		Board board = Board.BuildBoardA();
		_gameState.Island = new Island(board);

		//   And: Presence on A5 (city/coastal)
		var presenceSpace = board.Spaces.Single(s=>s.Label==startingPresence);
		_spirit.Given_HasPresenceOn(presenceSpace);

		//   And: Purchased WashAway
		_card = _spirit.Hand.Single(c => c.Name == WashAway.Name);
		_spirit.Energy = _card.Cost;
		_spirit.PlayCard( _card );

		// Jump to slow
		_spirit.Assert_CardIsReady(_card,Phase.Slow);

	}

}