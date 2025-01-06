namespace SpiritIsland.Tests.Spirits.SharpFangsNS;

public class SharpFangs_GrowthTests : BoardAGame {

	readonly SoloGameState gsbac;

	public SharpFangs_GrowthTests() : base( new SharpFangs() ) {
		gsbac = new SoloGameState( _spirit, _board );
		_gameState = gsbac;
		_gameState.Given_InitializedMinorDeck();

		// Setup for growth option B
		_spirit.Given_IsOn( _board[2] ); // wetlands
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

		await When_SharpFangsGrow( (_) => {
			User_GrowthA_ReclaimAll_Energy_DrawCard();
			User_GrowthB_PlacePresence();
			Ignore_CallForthPredators();
		});


		_spirit.Assert_AllCardsAvailableToPlay( 4+1);  // A
		_spirit.Assert_HasEnergy( 10 -1 + 1 );         // A
		// Assert_HasPowerProgressionCard( 0 );   // A

		_spirit.Assert_BoardPresenceIs( "A2:1,A3:1" );    // B
	}

	void Ignore_CallForthPredators() {
		User.NextDecision.HasPrompt("Select Growth").HasOptions("Call Forth Predators").ChooseFirst();
		User.NextDecision.HasPrompt("Replace 1 Presence with 1 Beast").Choose("Done");
	}

	[Fact]
	public async Task AC() {
		// a) cost -1, reclaim cards, gain +1 power card
		// c) gain power card, gain +1 energy

		_spirit.Given_HalfOfHandDiscarded();

		await When_SharpFangsGrow( (_) => {
			User_GrowthC_DrawCard_GainEnergy(); // gain 1 energy before we spend it
			User_GrowthA_ReclaimAll_Energy_DrawCard();
			Ignore_CallForthPredators();
		});

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

		await When_SharpFangsGrow( (_) => {
			User_GrowthD_GainEnergy();
			User_GrowthA_ReclaimAll_Energy_DrawCard();
			Ignore_CallForthPredators();
		});

		_spirit.Assert_AllCardsAvailableToPlay( 5);      // A
		// Assert_HasPowerProgressionCard( 0 );    // A
		_spirit.Assert_HasEnergy( 3-1+1 );      // A & D

	}

	[Fact]
	public async Task BC() {
		// b) add a presense to jungle or a land with beasts ( range 3)
		// c) gain power card, gain +1 energy

		await When_SharpFangsGrow( (_) => {
			User_GrowthB_PlacePresence();
			User_GrowthC_DrawCard_GainEnergy();
			Ignore_CallForthPredators();
		});

		// User.SkipsPresenceReplacementWithBeasts();

		_spirit.Assert_BoardPresenceIs( "A2:1,A3:1" );  // B
		_spirit.Assert_HasEnergy( 1 + 1 );         // C
		// Assert_HasPowerProgressionCard( 0 );    // A
	}

	[Fact]
	public async Task BD() {
		// b) add a presense to jungle or a land with beasts ( range 3)
		// d) +3 energy

		await When_SharpFangsGrow( (_) => {
			User_GrowthB_PlacePresence();
			User_GrowthD_GainEnergy();
			Ignore_CallForthPredators();
		});

		_spirit.Assert_BoardPresenceIs( "A2:1,A3:1" );  // B
		_spirit.Assert_HasEnergy( 3 + 1 );         // D
	}

	[Fact]
	public async Task CD() {
		// c) gain power card, gain +1 energy
		// d) +3 energy

		await When_SharpFangsGrow( (_) => {
			User_GrowthC_DrawCard_GainEnergy();
			User_GrowthD_GainEnergy();
			Ignore_CallForthPredators();
		});

		// Assert_HasPowerProgressionCard( 0 );    // C
		_spirit.Assert_HasEnergy( 1 + 3 + 1 );     // C + D
	}

	[Theory]
	[InlineData( 1, 1, "" )]
	[InlineData( 2, 1, "animal" )]
	[InlineData( 3, 1, "plant animal" )]
	[InlineData( 4, 2, "plant animal" )]
	[InlineData( 5, 2, "plant 2 animal" )]
	[InlineData( 6, 3, "plant 2 animal" )]
	[InlineData( 7, 4, "plant 2 animal" )]
	public Task EnergyTrack( int revealedSpaces, int expectedEnergyGrowth, string elements ) {
		return new SharpFangs().VerifyEnergyTrack(revealedSpaces, expectedEnergyGrowth, elements);
	}

	[Theory]
	[InlineData( 1, 2, 0 )]
	[InlineData( 2, 2, 0 )]
	[InlineData( 3, 3, 0 )]
	[InlineData( 4, 3, 1 )]
	[InlineData( 5, 4, 1 )]
	[InlineData( 6, 5, 2 )]
	public async Task CardTrack( int revealedSpaces, int expectedCardPlayCount, int reclaimCount ) {
		var spirit = new SharpFangs();
		await spirit.VerifyCardTrack(revealedSpaces, expectedCardPlayCount, "");
		spirit.Presence.RevealedActions.OfType<ReclaimN>().Count().ShouldBe(reclaimCount);
	}

	[Trait( "Spirit", "SetupAction" )]
	[Fact]
	public void HasSetUp() {
		var gs = new SoloGameState(new SharpFangs());
		gs.Initialize();
		gs.Spirit.GetAvailableActions( Phase.Init ).Count().ShouldBe( 1 );
	}

	async Task When_SharpFangsGrow(Action<VirtualUser> userAction) {
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
		User.Growth_SelectAction( $"Place Presence(3,{Filter.Beast}Or{Filter.Jungle})" );
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