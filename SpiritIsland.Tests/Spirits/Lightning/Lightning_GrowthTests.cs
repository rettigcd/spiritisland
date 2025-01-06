namespace SpiritIsland.Tests.Spirits.Lightning; 

[Collection("BaseGame Spirits")]
public class Lightning_GrowthTests : BoardAGame{

	public Lightning_GrowthTests()
		:base(new LightningsSwiftStrike())
	{}

	[Fact]
	public async Task Reclaim_Power_Energy() {
		// * reclaim, +1 power card, +1 energy

		_spirit.Given_HalfOfHandDiscarded();
		await _spirit.When_Growing( (user) => {
			user.Growth_SelectAction( "Gain Power Card" );

			// Select Minor card to draw
			_gameState.MinorCards.ShouldNotBeNull();
			user.SelectsMinorDeck();
			user.SelectMinorPowerCard();

		} );


		_spirit.Assert_AllCardsAvailableToPlay( 5 ); // drew a power card
		_spirit.Assert_HasEnergy( 1 + 1 ); // 1 from energy track

	}

	[Fact]
	public async Task Presense_Energy() {
		// +1 presense range 1, +3 energy

		_spirit.Given_IsOn( _board[1] );

		await _spirit.When_Growing( (user) => {
			user.Growth_SelectAction( "Place Presence(1)" );
			user.Growth_PlacesEnergyPresence( "A1;A2;A4;A5;A6" );
		} );

		Assert.Equal(1,_spirit.EnergyPerTurn);
		_spirit.Assert_HasEnergy( 3 + 1 ); // 1 from energy track
	}

	[Fact]
	public async Task TwoPresence(){
		// +1 presense range 2, +1 prsense range 0
		_spirit.Given_IsOn( _board[3] );

		await _spirit.When_Growing( 1, (user) => {
			user.Growth_PlacesEnergyPresence( "A1;A2;A3;A4;A5" );
			user.Growth_PlacesEnergyPresence( "A1;A2;A3;A4;A5" );
		} );

		_spirit.Assert_HasEnergy( 0 );

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
	public Task EnergyTrack(int revealedSpaces, int expectedEnergyGrowth) {
		return new LightningsSwiftStrike().VerifyEnergyTrack(revealedSpaces,expectedEnergyGrowth,"");
	}

	[Trait("Presence","CardTrack")]
	[Theory]
	[InlineDataAttribute(1,2)]
	[InlineDataAttribute(2,3)]
	[InlineDataAttribute(3,4)]
	[InlineDataAttribute(4,5)]
	[InlineDataAttribute(5,6)]
	public Task CardTrack( int revealedSpaces, int expectedCardPlayCount ) {
		return new LightningsSwiftStrike().VerifyCardTrack( revealedSpaces, expectedCardPlayCount, "" );
	}


}

