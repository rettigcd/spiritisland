namespace SpiritIsland.Tests.Spirits.SharpFangsNS;

public class SharpFangs_GrowthTests : BoardAGame {

	readonly GameState gsbac;

	public SharpFangs_GrowthTests() : base( new SharpFangs() ) {
		gsbac = new GameState( _spirit, _board );
		_gameState = gsbac;
		_gameState.Given_InitializedMinorDeck();

		// Setup for growth option B
		_spirit.Given_HasPresenceOnSpaces( _board[2] ); // wetlands
		_gameState.Tokens[ _board[7] ].Beasts.Init(1); // add beast to sand (not jungle)

	}

	// a) cost -1, reclaim cards, gain +1 power card
	// b) add a presense to jungle or a land with beasts
	// c) gain power card, gain +1 energy
	// d) +3 energy

	[Fact]
	public async Task AB() {
		_spirit.Energy = 10; 
		// a) cost -1, reclaim cards, gain +1 power card
		// b) add a presense to jungle or a land with beasts ( range 3)
		_spirit.Given_HalfOfHandDiscarded();

		await When_SharpFangsGrow( () => {
			User_GrowthA_ReclaimAll_Energy_DrawCard();
			User_GrowthB_PlacePresence();
		} );


		_spirit.Assert_AllCardsAvailableToPlay( 4+1);  // A
		_spirit.Assert_HasEnergy( 10 -1 + 1 );         // A
		// Assert_HasPowerProgressionCard( 0 );   // A

		_spirit.Assert_BoardPresenceIs( "A2:1,A3:1" );    // B
	}

	[Fact]
	public async Task AC() {
		// a) cost -1, reclaim cards, gain +1 power card
		// c) gain power card, gain +1 energy

		_spirit.Given_HalfOfHandDiscarded();

		await When_SharpFangsGrow( () => {
			User_GrowthC_DrawCard_GainEnergy(); // gain 1 energy before we spend it
			User_GrowthA_ReclaimAll_Energy_DrawCard();
		} );

		_spirit.Assert_AllCardsAvailableToPlay( 5 + 1 );  // A
		_spirit.Assert_HasEnergy( 0 + 1 );            // A & C
		// Assert_HasPowerProgressionCard( 0 );  // A
		// Assert_HasPowerProgressionCard( 1 );  // C
	}

	[Fact]
	public async Task AD() {
		// d) 3 energy
		// a) -1 energy, reclaim cards, gain +1 power card

		_spirit.Given_HalfOfHandDiscarded();

		await When_SharpFangsGrow( () => {
			User_GrowthD_GainEnergy();
			User_GrowthA_ReclaimAll_Energy_DrawCard();
		} );

		_spirit.Assert_AllCardsAvailableToPlay( 5);      // A
		// Assert_HasPowerProgressionCard( 0 );    // A
		_spirit.Assert_HasEnergy( 3-1+1 );      // A & D

	}

	[Fact]
	public async Task BC() {
		// b) add a presense to jungle or a land with beasts ( range 3)
		// c) gain power card, gain +1 energy

		await When_SharpFangsGrow( () => {
			User_GrowthB_PlacePresence();
			User_GrowthC_DrawCard_GainEnergy();
		} );

		// User.SkipsPresenceReplacementWithBeasts();

		_spirit.Assert_BoardPresenceIs( "A2:1,A3:1" );  // B
		_spirit.Assert_HasEnergy( 1 + 1 );         // C
		// Assert_HasPowerProgressionCard( 0 );    // A
	}

	[Fact]
	public async Task BD() {
		// b) add a presense to jungle or a land with beasts ( range 3)
		// d) +3 energy

		await When_SharpFangsGrow( () => {
			User_GrowthB_PlacePresence();
			User_GrowthD_GainEnergy();
		} );

		_spirit.Assert_BoardPresenceIs( "A2:1,A3:1" );  // B
		_spirit.Assert_HasEnergy( 3 + 1 );         // D
	}

	[Fact]
	public async Task CD() {
		// c) gain power card, gain +1 energy
		// d) +3 energy

		await When_SharpFangsGrow( () => {
			User_GrowthC_DrawCard_GainEnergy();
			User_GrowthD_GainEnergy();
		} );

		// Assert_HasPowerProgressionCard( 0 );    // C
		_spirit.Assert_HasEnergy( 1 + 3 + 1 );     // C + D
	}

	[Theory]
	[InlineDataAttribute( 1, 1, "" )]
	[InlineDataAttribute( 2, 1, "animal" )]
	[InlineDataAttribute( 3, 1, "plant animal" )]
	[InlineDataAttribute( 4, 2, "plant animal" )]
	[InlineDataAttribute( 5, 2, "plant 2 animal" )]
	[InlineDataAttribute( 6, 3, "plant 2 animal" )]
	[InlineDataAttribute( 7, 4, "plant 2 animal" )]
	public void EnergyTrack( int revealedSpaces, int expectedEnergyGrowth, string elements ) {
		var fix = new ConfigurableTestFixture { Spirit = new SharpFangs() };
		fix.VerifyEnergyTrack(revealedSpaces, expectedEnergyGrowth, elements);
	}

	[Theory]
	[InlineDataAttribute( 1, 2, 0 )]
	[InlineDataAttribute( 2, 2, 0 )]
	[InlineDataAttribute( 3, 3, 0 )]
	[InlineDataAttribute( 4, 3, 1 )]
	[InlineDataAttribute( 5, 4, 1 )]
	[InlineDataAttribute( 6, 5, 2 )]
	public void CardTrack( int revealedSpaces, int expectedCardPlayCount, int reclaimCount ) {
		var fix = new ConfigurableTestFixture { Spirit = new SharpFangs() };
		fix.VerifyCardTrack( revealedSpaces, expectedCardPlayCount, "" );
		fix.VerifyReclaim1Count(reclaimCount);
	}

	[Trait( "Spirit", "SetupAction" )]
	[Fact]
	public void HasSetUp() {
		var fxt = new ConfigurableTestFixture { Spirit = new SharpFangs() };
		fxt.GameState.Initialize();
		fxt.Spirit.GetAvailableActions( Phase.Init ).Count().ShouldBe( 1 );
	}

	async Task When_SharpFangsGrow(Action userAction) {
		_gameState.Phase = Phase.Growth;
		await _spirit.When_Growing(userAction);
	}

	void User_GrowthA_ReclaimAll_Energy_DrawCard() {
		User.Growth_SelectAction( "Reclaim All" );
		User.Growth_DrawsPowerCard();
		User.SelectsMinorDeck();
		User.SelectMinorPowerCard();
	}

	void User_GrowthB_PlacePresence() {
		User.Growth_SelectAction( $"PlacePresence(3,{Filter.Beast}Or{Filter.Jungle})" );
		User.PlacePresenceLocations( _spirit.Presence.Energy.RevealOptions.Single(), "A3;A7;A8" );
	}

	void User_GrowthC_DrawCard_GainEnergy() {
		User.Growth_SelectAction( "Gain Power Card" );
		User.SelectsMinorDeck();
		User.SelectMinorPowerCard();
	}

	void User_GrowthD_GainEnergy() {
		User.NextDecision.HasPrompt( "Select Growth" )
			.Choose( "Gain 3 Energy" );
	}


}