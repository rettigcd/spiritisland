namespace SpiritIsland.Tests.Spirits.River;

[Collection("BaseGame Spirits")]
public class RiversBounty_Tests {

	readonly GameState _gameState;
	readonly PowerCard _card;
	readonly Spirit _spirit;
	readonly VirtualUser _user;
	readonly Board _board;


	public RiversBounty_Tests() {
		_spirit = new RiverSurges();
		_user = new VirtualUser( _spirit );

		// A5 is the 'Y' land in the middle
		_gameState = new SoloGameState( _spirit, Boards.A );

		//   And: a game on Board-A
		_board = Boards.A;
		_gameState.Island = new Island( _board );
		_gameState.Phase = Phase.Slow;

		//   And: Presence on A4
		_spirit.Given_IsOn( _board[4] );

		//   And: Purchased WashAway
		_card = _spirit.Hand.Single( c => c.Title == RiversBounty.Name );
		_spirit.Energy = _card.Cost;
		_spirit.PlayCard( _card );

		// Jump to slow
		_spirit.Assert_CardIsReady( _card, Phase.Slow );

	}

	[Fact]
	public void Stats() {
		var card = PowerCard.For(typeof(RiversBounty));
		card.Assert_CardStatus( 0, Phase.Slow, "sun water animal" );
	}

	SpaceSpec Given_SpiritHas1Presence() => ActionScope.Current.Spaces.Where( _spirit.Presence.IsOn ).Single().SpaceSpec;

	// 1 target, 0 dahan, 1 to gather       => resolved, dahan gathered, no child
	// 1 target, 1 dahan, 1 to gather        => resolved, dahan gathered, child!
	// 1 target, 2 dahan, nothing to gather  => resolved, child!
	[Theory]
	[InlineData(0,0,0,0)]
	[InlineData(1,0,1,0)]
	[InlineData(1,1,3,1)]
	[InlineData(1,2,4,1)]
	[InlineData(2,1,4,1)]
	public async Task DahanComingSameLand(
		int startingCount, 
		int dahanToGather, 
		int endingCount,
		int endingEnergy
	) {
		// Given: spirit has 1 presence
		SpaceSpec target = Given_SpiritHas1Presence();

		//   And: presence space has dahan
		Given_AddDahan( startingCount, target );

		//   And: neighbors have some dahan
		SpaceSpec neighbor = target.Adjacent_Existing.First();
		Given_AddDahan( dahanToGather, neighbor );

		await _spirit.When_ResolvingCard<RiversBounty>( u => {
			u.NextDecision.HasTargetSpacePrompt( RiversBounty.Name ).HasOptions( "A4" ).Choose( "A4" );
			if(0<dahanToGather)
				u.NextDecision.HasPrompt($"Gather up to ({dahanToGather})")
					.HasSourceOptions("D@2,Done").MoveFrom("D@2");
			if(1<dahanToGather)
				u.NextDecision.HasPrompt("Gather up to (1)")
					.HasSourceOptions("D@2,Done").MoveFrom("D@2");
		} ).ShouldComplete();

		Assert_DahanCount( target, endingCount );
		_spirit.Energy.ShouldBe( endingEnergy );
	}

	void Assert_DahanCount( SpaceSpec target, int endingCount ) {
		_gameState.Tokens[target].Dahan.CountAll.ShouldBe( endingCount ); // same as original
	}

	[Fact]
	public async Task DahanComingDifferentLands() {
		// Given: spirit has 1 presence
		SpaceSpec target = Given_SpiritHas1Presence();

		//   And: neighbors have 1 dahan each 
		const int dahanToGather = 2;
		var neighbors = target.Adjacent_Existing.ToArray();
		for(int i=0;i<dahanToGather;++i)
			Given_AddDahan( 1, neighbors[i] );

		await _card.ActivateAsync( _spirit ).AwaitUser( user => {
			user.TargetsLand( RiversBounty.Name, "A4" );
			user.NextDecision.HasPrompt("Gather up to (2)").MoveFrom("D@2 on A1","D@2 on A1,D@2 on A2,Done");
			user.NextDecision.HasPrompt("Gather up to (1)").MoveFrom("D@2","D@2,Done");
		} ).ShouldComplete();

		Assert_DahanCount( target, 3 );
	}

	[Fact]
	public async Task DamagedDahanComingDifferentLands() {
		// Given: spirit has 1 presence
		SpaceSpec target = Given_SpiritHas1Presence();

		//   And: neighbors have 1 damaged dahan each 
		const int dahanToGather = 2;
		var ctx = _spirit.Target( target );
		Space[] neighbors = ctx.Adjacent.ToArray();
		for(int neighborIndex = 0; neighborIndex<dahanToGather; ++neighborIndex)
			neighbors[neighborIndex].Init( StdTokens.Dahan1, 1 );

		await _spirit.When_ResolvingCard<RiversBounty>( u => {
			u.NextDecision.HasTargetSpacePrompt( RiversBounty.Name ).HasOptions( "A4" ).Choose( "A4" );
			u.NextDecision.HasPrompt("Gather up to (2)").MoveFrom("D@1 on A1","D@1 on A1,D@1 on A2,Done");
			u.NextDecision.HasPrompt("Gather up to (1)").MoveFrom("D@1","D@1,Done");
		} ).ShouldComplete();


		Assert_DahanCount( target, 3 );
	}


	[Fact]
	public async Task TwoPresenceSpaces(){
		// Given: spirit has presence on A4 && A8
		_spirit.Given_IsOn(_board[8]);
		Space[] targetOptions = _spirit.Presence.Lands.ToArray();
		Assert.Equal(2,targetOptions.Length);

		//   And: 2 dahan in A5 (touches both)
		Given_AddDahan(2,_board[5]);

		await _spirit.When_ResolvingCard<RiversBounty>( u => {
			u.NextDecision.HasTargetSpacePrompt( RiversBounty.Name ).HasOptions( "A4,A8" ).Choose( "A4" );
			u.NextDecision.HasPrompt("Gather up to (2)").MoveFrom("D@2","D@2,Done");
			u.NextDecision.HasPrompt("Gather up to (1)").MoveFrom("D@2","D@2,Done");
		} ).ShouldComplete();

		Assert_DahanCount( _board[4], 3 );
	}

	[Fact]
	public async Task TwoDahanOnPresenceSpace(){
		// Given: spirit has presence on A4
		var targetOptions = _spirit.Presence.Lands.ToArray();
		Assert.Single( targetOptions );

		//   And: 2 dahan in A5 (touches both)
		Given_AddDahan(2,_board[4]);

		Task t = _card.ActivateAsync( _spirit );

		// Select 1st land
		_user.TargetsLand( RiversBounty.Name,"A4" );

		await t.ShouldComplete();

	}

	[Fact]
	public void DahanCountIncludesDamaged() {
		// This is a nice test, but it is too close to the implementation.  Refactoring might not use ctx.DahanCount
		var space = _gameState.Island.Boards[0][4];
		var tokens = _gameState.Tokens[space];
		var dahan = tokens.Dahan;
		tokens.Init( StdTokens.Dahan1, 5 );
		dahan.Init(7);
		tokens.Dahan.CountAll.ShouldBe(12);
	}

	void Given_AddDahan( int startingCount, SpaceSpec target ) {
		target.ScopeSpace.Dahan.Init( startingCount );
		Assert_DahanCount( target, startingCount );
	}

}