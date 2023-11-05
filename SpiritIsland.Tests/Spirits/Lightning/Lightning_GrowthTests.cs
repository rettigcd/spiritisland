namespace SpiritIsland.Tests.Spirits.Lightning; 

public class Lightning_GrowthTests : GrowthTests{

	public Lightning_GrowthTests()
		:base(new LightningsSwiftStrike())
	{}

	[Fact]
	public async Task Reclaim_Power_Energy() {
		// * reclaim, +1 power card, +1 energy

		Given_HalfOfPowercardsPlayed();
		await _spirit.When_Growing( () => {
			User.Growth_SelectAction( "Gain Power Card" );

			// Select Minor card to draw
			_gameState.MinorCards.ShouldNotBeNull();
			User.SelectsMinorDeck();
			User.SelectMinorPowerCard();

		} );


		Assert_AllCardsAvailableToPlay( 5 ); // drew a power card
		Assert_HasEnergy( 1 + 1 ); // 1 from energy track

	}

	[Fact]
	public async Task Presense_Energy() {
		// +1 presense range 1, +3 energy

		Given_HasPresence( _board[1] );

		await _spirit.When_Growing( () => {
			User.Growth_SelectAction( "PlacePresence(1)" );
			User.Growth_PlacesEnergyPresence( "A1;A2;A4;A5;A6" );
		} );

		Assert.Equal(1,_spirit.EnergyPerTurn);
		Assert_HasEnergy( 3 + 1 ); // 1 from energy track
	}

	[Fact]
	public async Task TwoPresence(){
		// +1 presense range 2, +1 prsense range 0
		Given_HasPresence( _board[3] );

		await _spirit.When_Growing( 1, () => {
			User.Growth_PlacesEnergyPresence( "A1;A2;A3;A4;A5" );
			User.Growth_PlacesEnergyPresence( "A1;A2;A3;A4;A5" );
		} );

		Assert_HasEnergy( 0 );

	}

	[Trait("Presence","EnergyTrack")]
	[Theory]
	[InlineDataAttribute(1,1)]
	[InlineDataAttribute(2,1)]
	[InlineDataAttribute(3,2)]
	[InlineDataAttribute(4,2)]
	[InlineDataAttribute(5,3)]
	[InlineDataAttribute(6,4)]
	[InlineDataAttribute(7,4)]
	[InlineDataAttribute(8,5)]
	public async Task EnergyTrack(int revealedSpaces, int expectedEnergyGrowth) {
		var fixture = new ConfigurableTestFixture { Spirit = new LightningsSwiftStrike() };
		await fixture.VerifyEnergyTrack(revealedSpaces,expectedEnergyGrowth,"");
	}

	[Trait("Presence","CardTrack")]
	[Theory]
	[InlineDataAttribute(1,2)]
	[InlineDataAttribute(2,3)]
	[InlineDataAttribute(3,4)]
	[InlineDataAttribute(4,5)]
	[InlineDataAttribute(5,6)]
	public async Task CardTrack( int revealedSpaces, int expectedCardPlayCount ) {
		var fixture = new ConfigurableTestFixture { Spirit = new LightningsSwiftStrike() };
		await fixture.VerifyCardTrack( revealedSpaces, expectedCardPlayCount, "" );
	}


}

