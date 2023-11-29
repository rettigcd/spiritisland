namespace SpiritIsland.Tests.Spirits.River;

[Collection("BaseGame Spirits")]
public class WashAway_Tests : SpiritCards_Tests {

	public WashAway_Tests():base(new RiverSurges() ) { }

	[Fact]
	public void Nothing() {
		UserGateway.UsePreselect.Value = true;

		Given_RiverPlayingWashAway();
		_gameState.Phase = Phase.Slow;

		// no explorers

		// If not using pre-selects, will still target a space.

		// When_PlayingCard();
		_ = _card.ActivateAsync( _spirit.BindMyPowers() );
		//task.IsCompleted.ShouldBeTrue();

		User.Assert_Done();

	}

	[Trait("Feature","Push")]
	[Theory]
	[InlineData(1,0,0,"","1E@1")]
	[InlineData(0,1,0,"","1T@2")]
	[InlineData(1,0,1,"1C@3","1E@1")]
	public void OneTarget1PushableType(int explorerCount, int townCount, int cityCount, string expectedTargetResult, string expectedDestinationResult) {
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

		When_PlayingCard();

		//		User.TargetsLand( WashAway.Name, targetSpace.Label );
		//		User.OptionallyPushesInvaderTo(invader.Token.ToString(),"A1,[A2],A3,A5");

		var invader = _spirit.Portal.Next.Options[0] as SpaceToken;
		var invaderText = $"{invader.Token.Text}";

		User.AssertDecision( PreselectPrompt, invaderText + ",Done", invaderText );
		User.AssertDecisionInfo( $"Push {invader.Token.Text} to", "A1,[A2],A3,A5" );

		// check that explore was moved
		_gameState.Assert_Invaders(targetSpace, expectedTargetResult );
		_gameState.Assert_Invaders( board[2], expectedDestinationResult );
	}

	// WashAway: Multiple target lands

	// WashAway: into ocean?
	[Trait("Feature","Push")]
	[Fact]
	public void DoesntPushIntoOcean(){
		UserGateway.UsePreselect.Value = true;

		Given_RiverPlayingWashAway( "A4");
		_gameState.Phase = Phase.Slow;

		// 1 explorer on A2
		var board = _gameState.Island.Boards[0];
		Space targetSpace = board[2];
		_gameState.Tokens[targetSpace].AdjustDefault(Human.Explorer, 1);

		When_PlayingCard();

		// User.TargetsLand( WashAway.Name, targetSpace.Label );
		User.AssertDecision( PreselectPrompt, "E@1,Done", "E@1" );
		User.AssertDecisionInfo( "Push E@1 to", "A1,[A3],A4" );
	}
	const string PreselectPrompt = "Push up to (3)";

	[Trait("Feature","Push")]
	[Fact]
	public async Task One1Target2PushableTypes() {
		UserGateway.UsePreselect.Value = true;
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
			u.NextDecision.HasPrompt( "Push up to (3)" ).HasOptions( "E@1,T@2,Done" ).Choose( "E@1" );
			u.NextDecision.HasPrompt("Push E@1 to").HasOptions("A1,A2,A3,A5").Choose("A2");
			u.NextDecision.HasPrompt( "Push up to (1)" ).HasOptions( "T@2,Done" ).Choose( "T@2" );
			u.NextDecision.HasPrompt("Push T@2 to").HasOptions("A1,A2,A3,A5").Choose("A3");
		} ).ShouldComplete();

		// check that explore was moved
		_gameState.Assert_Invaders(targetSpace,"");
		_gameState.Assert_Invaders(explorerDestination, "1E@1" );
		_gameState.Assert_Invaders(townDestination, "1T@2" );
	}

	[Trait("Feature","Push")]
	[Fact]
	public void DamagedTown() {
		UserGateway.UsePreselect.Value = true;

		Given_RiverPlayingWashAway();
		_gameState.Phase = Phase.Slow;

		// 1 damaged town on A4
		var board = _gameState.Island.Boards[0];
		Space targetSpace = board[4];
		_gameState.Tokens[targetSpace].Adjust( StdTokens.Town1, 1 );

		var invaderDestination = board[2];

		When_PlayingCard();

		// First Invader is different (contains space) due to pre-select
		User.AssertDecision( PreselectPrompt, "T@1,Done", "T@1" );
		User.AssertDecisionInfo( "Push T@1 to", "A1,[A2],A3,A5" );
		User.Assert_Done();

		// check that explore was moved
		_gameState.Assert_Invaders( targetSpace, "" );
		_gameState.Assert_Invaders( invaderDestination, "1T@1" );
	}

	// WashAway: push 3 different invaders to 3 different lands
	// WashAway: multiple invader types
	[Trait("Feature","Push")]
	[Fact]
	public async Task Push3InvadersToDifferentLands() {
		UserGateway.UsePreselect.Value = true;

		Given_RiverPlayingWashAway();
		_gameState.Phase = Phase.Slow;

		// 31 explorers on A4
		var board = _gameState.Island.Boards[0];
		Space targetSpace = board[4];
		_gameState.Tokens[ targetSpace ].AdjustDefault( Human.Explorer, 3 );

		//  When: activating card
		Task t = When_PlayingCard();

		// First Invader is different (contains space) due to pre-select
		User.AssertDecision( PreselectPrompt, "E@1,Done", "E@1" );
		User.AssertDecisionInfo( "Push E@1 to", "A1,[A2],A3,A5" );
		// remaining are normal since space is already selected.
		User.AssertDecision( "Push up to (2)", "E@1,Done", "E@1" );
		User.AssertDecisionInfo( "Push E@1 to", "A1,A2,[A3],A5" );
		User.AssertDecision( "Push up to (1)", "E@1,Done", "E@1" );
		User.AssertDecisionInfo( "Push E@1 to", "A1,A2,A3,[A5]" );

		await t.ShouldComplete();

		// check that explore was moved
		_gameState.Assert_Invaders( targetSpace,"" );
		_gameState.Assert_Invaders( board[2], "1E@1" );
		_gameState.Assert_Invaders( board[3], "1E@1" );
		_gameState.Assert_Invaders( board[5], "1E@1" );
	}

	void Given_RiverPlayingWashAway(string startingPresence="A5") {
		// A5 is the 'Y' land in the middle
		Given_GameWithSpirits(_spirit);

		//   And: a game on Board-A
		Board board = Board.BuildBoardA();
		_gameState.Island = new Island(board);

		//   And: Presence on A5 (city/coastal)
		var presenceSpace = board.Spaces.Single(s=>s.Label==startingPresence);
		_spirit.Given_HasPresenceOn(presenceSpace);

		//   And: Purchased WashAway
		_card = _spirit.Hand.Single(c => c.Name == WashAway.Name);
		_spirit.Energy = _card.Cost;
		PlayCard();

		// Jump to slow
		Assert_CardIsReady(_card,Phase.Slow);

	}

}