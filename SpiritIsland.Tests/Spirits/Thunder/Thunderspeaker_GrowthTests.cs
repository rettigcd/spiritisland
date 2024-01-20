namespace SpiritIsland.Tests.Spirits.Thunder;

public class Thunderspeaker_GrowthTests : BoardAGame{

	public Thunderspeaker_GrowthTests():base( new Thunderspeaker() ) {}

	[Fact]
	public async Task ReclaimAnd2PowerCards() {
		// Growth Option 1 - Reclaim All, +2 Power cards
		_spirit.Given_HalfOfHandDiscarded();

		await _spirit.When_Growing( () => {
			User.Growth_DrawsPowerCard();
			User.SelectsMinorDeck();
			User.SelectMinorPowerCard();

			User.Growth_DrawsPowerCard();
			User.SelectsMinorDeck();
			User.SelectMinorPowerCard();
		} );

		_spirit.Assert_AllCardsAvailableToPlay( 6);
		_spirit.Assert_HasEnergy(1);
	}

	[Theory]
	[InlineData( "3,5,8", "A3;A5" )]
	[InlineData( "3,4,8", "A3;A4" )]
	[InlineData( "4,8", "A4" )]
	[InlineData( "1,4,8", "A1;A4" )]
	public async Task TwoPresence( string initialDahanSquares, string expectedPresenseOptions ) {
		// +1 presense within 2 - contains dahan
		// +1 presense within 1 - contains dahan
		_spirit.Given_HasPresenceOnSpaces( _board[3] );
		//	 And: dahan on initial spot
		foreach(string s in initialDahanSquares.Split( ',' ))
			_board[int.Parse( s )].Tokens.Dahan.Init(1);

		await _spirit.When_Growing( 1, () => {
			User.Growth_PlacesEnergyPresence( expectedPresenseOptions );
			User.Growth_PlacesEnergyPresence( expectedPresenseOptions );
		} );

		_spirit.Assert_HasEnergy( 0 );

	}

	[Fact]
	public async Task PresenseAndEnergy() {
		// +1 presense within 1, +4 energy
		_spirit.Given_HasPresenceOnSpaces( _board[1] );

		await _spirit.When_Growing( () => {
			User.Growth_SelectAction( "PlacePresence(1)" );
			User.Growth_PlacesEnergyPresence( "A1;A2;A4;A5;A6" );
		});

		Assert.Equal(1,_spirit.EnergyPerTurn);
		_spirit.Assert_HasEnergy( 4+1 );

	}

	[Trait("Presence","EnergyTrack")]
	[Theory]
	[InlineDataAttribute(1,1,"")]
	[InlineDataAttribute(2,1,"air")]
	[InlineDataAttribute(3,2,"air")]
	[InlineDataAttribute(4,2, "fire air" )]
	[InlineDataAttribute(5,2, "sun fire air" )]
	[InlineDataAttribute(6,3, "sun fire air" )]
	public void EnergyTrack(int revealedSpaces, int expectedEnergyGrowth, string elements ) {
		var fix = new ConfigurableTestFixture { Spirit = new Thunderspeaker() };
		fix.VerifyEnergyTrack( revealedSpaces, expectedEnergyGrowth, elements );
	}

	[Trait("Presence","CardTrack")]
	[Theory]
	[InlineDataAttribute(1,1,0)]
	[InlineDataAttribute(2,2,0)]
	[InlineDataAttribute(3,2,0)]
	[InlineDataAttribute(4,3,0)]
	[InlineDataAttribute(5,3,1)]
	[InlineDataAttribute(6,3,1)]
	[InlineDataAttribute(7,4,1)]
	public void CardTrack(int revealedSpaces, int expectedCardPlayCount, int reclaimCount ) {
		var fix = new ConfigurableTestFixture { Spirit = new Thunderspeaker() };
		fix.VerifyCardTrack( revealedSpaces, expectedCardPlayCount, "" );
		fix.VerifyReclaim1Count( reclaimCount );
	}

}

