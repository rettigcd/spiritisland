namespace SpiritIsland.Tests.Spirits.Thunder;

public class Thunderspeaker_GrowthTests : BoardAGame{

	public Thunderspeaker_GrowthTests():base( new Thunderspeaker() ) {}

	[Fact]
	public async Task ReclaimAnd2PowerCards() {
		// Growth Option 1 - Reclaim All, +2 Power cards
		_spirit.Given_HalfOfHandDiscarded();

		await _spirit.When_Growing( (user) => {
			user.Growth_DrawsPowerCard();
			user.SelectsMinorDeck();
			user.SelectMinorPowerCard();

			user.Growth_DrawsPowerCard();
			user.SelectsMinorDeck();
			user.SelectMinorPowerCard();
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
		_spirit.Given_IsOn( _board[3] );
		//	 And: dahan on initial spot
		foreach(string s in initialDahanSquares.Split( ',' ))
			_board[int.Parse( s )].ScopeSpace.Dahan.Init(1);

		await _spirit.When_Growing( 1, (user) => {
			user.Growth_PlacesEnergyPresence( expectedPresenseOptions );
			user.Growth_PlacesEnergyPresence( expectedPresenseOptions );
		} );

		_spirit.Assert_HasEnergy( 0 );

	}

	[Fact]
	public async Task PresenseAndEnergy() {
		// +1 presense within 1, +4 energy
		_spirit.Given_IsOn( _board[1] );

		await _spirit.When_Growing( (user) => {
			user.Growth_SelectAction( "Place Presence(1)" );
			user.Growth_PlacesEnergyPresence( "A1;A2;A4;A5;A6" );
		});

		Assert.Equal(1,_spirit.EnergyPerTurn);
		_spirit.Assert_HasEnergy( 4+1 );

	}

	[Trait("Presence","EnergyTrack")]
	[Theory]
	[InlineData(1,1,"")]
	[InlineData(2,1,"air")]
	[InlineData(3,2,"air")]
	[InlineData(4,2, "fire air" )]
	[InlineData(5,2, "sun fire air" )]
	[InlineData(6,3, "sun fire air" )]
	public Task EnergyTrack(int revealedSpaces, int expectedEnergyGrowth, string elements ) {
		return new Thunderspeaker().VerifyEnergyTrack( revealedSpaces, expectedEnergyGrowth, elements );
	}

	[Trait("Presence","CardTrack")]
	[Theory]
	[InlineData(1,1,0)]
	[InlineData(2,2,0)]
	[InlineData(3,2,0)]
	[InlineData(4,3,0)]
	[InlineData(5,3,1)]
	[InlineData(6,3,1)]
	[InlineData(7,4,1)]
	public async Task CardTrack(int revealedSpaces, int expectedCardPlayCount, int reclaimCount ) {
		var spirit = new Thunderspeaker();
		await spirit.VerifyCardTrack(revealedSpaces, expectedCardPlayCount, "");
		spirit.Presence.RevealedActions.OfType<ReclaimN>().Count().ShouldBe(reclaimCount);
	}

}

