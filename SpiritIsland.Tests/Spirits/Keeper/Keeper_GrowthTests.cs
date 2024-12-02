namespace SpiritIsland.Tests.Spirits.KeeperNS;

public class Keeper_GrowthTests : BoardAGame {

	readonly SoloGameState gsbac;

	public Keeper_GrowthTests() : base( new Keeper() ) {
		gsbac = new SoloGameState( _spirit, _board );
		_gameState = gsbac;
		_gameState.Given_InitializedMinorDeck();
	}

	// a) reclaim, +1 energy
	// b) +1 power card
	// c) add presense range 3 containing wilds or presence, +1 energy
	// d) -3 energy, +1 power card, add presense to land without blight range 3

	[Fact]
	public async Task A_Reclaim_Energy_B_Powercard() {
		// a) reclaim, +1 energy
		// b) +1 power card
		_spirit.Given_HalfOfHandDiscarded();

		await _spirit.When_Growing( (user) => {
			User_Activates_A(user);
			User_Activates_B(user);
		} );

		_spirit.Assert_AllCardsAvailableToPlay( 1 + 4 );
		_spirit.Assert_HasEnergy( 1 + 2 );
		// Assert_HasPowerProgressionCard( 0 );
	}

	[Fact]
	public async Task A_Reclaim_Energy_C_Presence_Energy() {
		// a) reclaim, +1 energy
		// c) add presense range 3 containing (wilds or presense), +1 energy

		_spirit.Given_IsOn( _board[3] );
		_spirit.Given_HalfOfHandDiscarded();
		Given_HasWilds( _board[8] ); // 3 spaces away

		_gameState.Phase = Phase.Growth;

		await _spirit.When_Growing( (user) => {
			User_Activates_A(user);
			User_Activates_C(user);
		} );

		_spirit.Assert_AllCardsAvailableToPlay();      // A
		_spirit.Assert_HasEnergy( 2 + 2 );             // A & C
		_spirit.Assert_BoardPresenceIs( "A3:2" );      // C
	}


	[Fact]
	public async Task A_Reclaim_Energy_D_Presence_PowerCard_LoseEnergy() {
		_spirit.Energy = 10; // so we can -3 it
		// a) reclaim, +1 energy
		// d) -3 energy, +1 power card, add presense to land without blight range 3

		// Given: presence on board A  (default island is Board A)
		_spirit.Given_HalfOfHandDiscarded();
		_spirit.Given_IsOn( _board[3] );
		Given_BlightEverywhereExcept7();

		await _spirit.When_Growing( (user) => {
			User_Activates_A(user);
			User_Activates_D(user);
		} );

		_spirit.Assert_AllCardsAvailableToPlay( 4+1);     // A
		_spirit.Assert_HasEnergy( 10 + 2-3+1 );                // A & D
		// Assert_HasPowerProgressionCard(0); // D
		_spirit.Assert_BoardPresenceIs( "A3:1,A7:1" );        // D

	}

	[Fact]
	public async Task B_Powercard_C_Presence_Energy() {
		// b) +1 power card
		// c) add presense range 3 containing (wilds or presense), +1 energy

		// Given: presence at A3  (default island is Board A)
		_spirit.Given_IsOn( _board[3] );
		// Given: 1 wilds, 3 away
		Given_HasWilds( _board[8] );

		await _spirit.When_Growing( (user) =>{
			User_Activates_B(user);
			User_Activates_C(user);
		} );

		// Assert_HasPowerProgressionCard( 0); // B
		_spirit.Assert_HasEnergy( 1 + 2 );             // C
		_spirit.Assert_BoardPresenceIs( "A3:2" );     // C
	}

	[Fact]
	public async Task B_Powercard_D_Presence_PowerCard_LoseEnergy() {
		// b) +1 power card
		// d) -3 energy, +1 power card, add presense to land without blight range 3

		// Given: presence on board A  (default island is Board A)
		_spirit.Given_IsOn( _board[3] );
		Given_BlightEverywhereExcept7();
		_spirit.Energy = 10; // so we can do this option

		_gameState.Phase = Phase.Growth;
		await _spirit.When_Growing( (user) => {
			User_Activates_B(user);
			User_Activates_D(user);
		} );

		// Assert_HasPowerProgressionCard( 0); // B
		// Assert_HasPowerProgressionCard( 1 ); // B
		_spirit.Assert_HasCardAvailable( BoonOfGrowingPower.Name ); // D
		_spirit.Assert_BoardPresenceIs( "A3:1,A7:1" );     // D
		_spirit.Assert_HasEnergy( 10 + 2 - 3 );
	}

	[Fact]
	public async Task C_Presence_Energy_D_Presence_PowerCard_LoseEnergy() {
		const int startingEnergy = 10;
		_spirit.Energy = startingEnergy;
		// c) add presense range 3 containing (wilds or presense), +1 energy
		// d) -3 energy, +1 power card, add presense to land without blight range 3

		// Given: presence on board A  (default island is Board A)
		_spirit.Given_IsOn( _board[3] );
		Given_HasWilds( _board[8] );
		Given_BlightEverywhereExcept7();

		await _spirit.When_Growing( (user) => {
			User_Activates_C(user);
			User_Activates_D(user);
		} );

		_spirit.Assert_HasEnergy( startingEnergy + _spirit.EnergyPerTurn - 2  );          // C & D
		// Assert_HasPowerProgressionCard(0); // D

	}

	[Trait("Feature","Push")]
	[Fact]
	public async Task SacredSitesPushDahan() {
		SpaceSpec spaceSpec = _board[5];
		var space = spaceSpec.ScopeSpace;
		// Given: space with 2 dahan
		spaceSpec.Given_HasTokens("2D@2");
		//   and presence on that space
		_spirit.Given_IsOn(spaceSpec);

		// When: we place a presence on that space
		await _spirit.Presence.Token.MoveAsync( _spirit.Presence.Energy.RevealOptions.Single(), space )
			.AwaitUser( user => {
				user.NextDecision.HasPrompt("Push (2)").MoveFrom("D@2").MoveTo("A4","A1,A4,A6,A7,A8");
				user.NextDecision.HasPrompt("Push (1)").MoveFrom("D@2").MoveTo("A7","A1,A4,A6,A7,A8");
			} )
			.ShouldComplete();

		_spirit.Presence.SacredSites.ShouldContain(space);
		_gameState.Tokens[spaceSpec].Dahan.CountAll.ShouldBe(0,"SS should push dahan from space");
	}


	[Theory]
	[InlineDataAttribute( 1, 2, "" )]
	[InlineDataAttribute( 2, 2, "sun" )]
	[InlineDataAttribute( 3, 4, "sun" )]
	[InlineDataAttribute( 4, 5, "sun" )]
	[InlineDataAttribute( 5, 5, "sun plant" )]
	[InlineDataAttribute( 6, 7, "sun plant" )]
	[InlineDataAttribute( 7, 8, "sun plant" )]
	[InlineDataAttribute( 8, 9, "sun plant" )]
	public Task EnergyTrack( int revealedSpaces, int expectedEnergyGrowth, string elements ) {
		var fix = new ConfigurableTestFixture { Spirit = new Keeper() };
		return fix.VerifyEnergyTrack( revealedSpaces, expectedEnergyGrowth, elements );
	}

	[Theory]
	[InlineDataAttribute( 1, 1 )]
	[InlineDataAttribute( 2, 2 )]
	[InlineDataAttribute( 3, 2 )]
	[InlineDataAttribute( 4, 3 )]
	[InlineDataAttribute( 5, 4 )]
	[InlineDataAttribute( 6, 5 )]
	public Task CardTrack( int revealedSpaces, int expectedCardPlayCount ) {
		var fix = new ConfigurableTestFixture { Spirit = new Keeper() };
		return fix.VerifyCardTrack(revealedSpaces,expectedCardPlayCount,"");
	}

	void AddBlight( SpaceSpec space ) {
		_gameState.Tokens[ space ].Blight.Adjust( 1 );
	}

	void Given_BlightEverywhereExcept7() {
		AddBlight( _board[1] );
		AddBlight( _board[2] );
		AddBlight( _board[3] );
		AddBlight( _board[4] );
		AddBlight( _board[5] );
		AddBlight( _board[6] );
		AddBlight( _board[8] );
		_gameState.Tokens[ _board[7] ].Blight.Count.ShouldBe( 0 );
	}

	void Given_HasWilds( SpaceSpec space ) {
		_gameState.Tokens[space].Wilds.Init(1);
	}

	static void User_Activates_A(VirtualUser user) {
		user.Growth_SelectAction( "Reclaim All" );
	}

	static void User_Activates_B(VirtualUser user) {
		user.Growth_DrawsPowerCard();
		user.SelectsMinorDeck();
		user.SelectMinorPowerCard();
	}

	static void User_Activates_C(VirtualUser user) {
		user.Growth_SelectAction( $"Place Presence(3,{Filter.Presence}Or{Filter.Wilds})" );
		user.Growth_PlacesEnergyPresence( "A3;A8" );
	}

	static void User_Activates_D(VirtualUser user) {
		user.Growth_SelectAction( "Place Presence(3,No Blight)" );
		user.PlacesCardPlayPresence( "A7" );
		user.Growth_DrawsPowerCard();
		user.SelectsMinorDeck();
		user.SelectMinorPowerCard();
	}

}