namespace SpiritIsland.Tests.Spirits.BringerNS;

[Collection("BaseGame Spirits")]
public class Bringer_GrowthTests : BoardAGame {

	public Bringer_GrowthTests():base( new Bringer() ) {}

	[Fact] 
	public async Task ReclaimAll_PowerCard() { // Growth Option 1

		// reclaim, +1 power card
		_spirit.Given_HalfOfHandDiscarded();

		await _spirit.When_Growing( 0, (user) => {
			user.SelectsMinorDeck();
			user.SelectMinorPowerCard();
		} );

		// Then:
		_spirit.Assert_AllCardsAvailableToPlay( 4 + 1 );
		_spirit.Assert_HasCardAvailable( "Drought" );

	}

	[Fact] 
	public async Task Reclaim1_Presence() { // Growth Option 2
		// reclaim 1, add presense range 0
		_spirit.Given_HalfOfHandDiscarded();
		_spirit.Given_IsOn( _board[4] );

		await _spirit.When_Growing( 1, (user)=> {
			user.Growth_Reclaims1( "Predatory Nightmares $2 (Slow),[Dreams of the Dahan $0 (Fast)]" );
			user.Growth_PlacesPresence( "energy>A4" );
		} );

		_spirit.Hand.Count.ShouldBe( 3 );
	}

	[Fact] 
	public async Task PowerCard_Presence() { // Growth Option 3
		// +1 power card, +1 pressence range 1
		_spirit.Given_IsOn( _board[1] );

		await _spirit.When_Growing( 2, (user)=> {
			user.Growth_DrawsPowerCard();
			user.SelectsMinorDeck();
			user.SelectMinorPowerCard();
			user.Growth_PlacesEnergyPresence( "A1;A2;A4;A5;A6" );
		} );

		Assert_GainsFirstMinorCard();
		_spirit.Assert_BoardPresenceIs( "A1:2" );
	}

	[Fact] 
	public async Task PresenseOnPieces_Energy(){ // Growth Option 4

		_board = LineBoard.MakeBoard();
		_gameState = new GameState( _spirit, _board );

		_spirit.Given_IsOn(_board[5]);
		_board[6].ScopeSpace.Dahan.Init(1);
		_board[7].ScopeSpace.AdjustDefault( Human.Explorer, 1 );
		_board[8].ScopeSpace.AdjustDefault( Human.Town, 1 );
		_board[0].ScopeSpace.AdjustDefault( Human.City, 1 );

		// add presense range 4 Dahan or Invadors, +2 energy
		await _spirit.When_Growing( (user) => {
			// User.Growth_SelectsOption( "GainEnergy(2) / PlacePresence(4,dahan or invaders)" );
			user.Growth_SelectAction( $"Place Presence(4,{Filter.Dahan}Or{Filter.Invaders})" );
			user.Growth_PlacesEnergyPresence( "T6;T7;T8;T9" );
		} );

		Assert.Equal(2,_spirit.EnergyPerTurn);
		_spirit.Assert_HasEnergy(2+2);
		_spirit.Assert_BoardPresenceIs("T5:1,T6:1");
	}

	[Trait("Presence","EnergyTrack")]
	[Theory]
	[InlineDataAttribute(1,2,"")]
	[InlineDataAttribute(2,2,"air")]
	[InlineDataAttribute(3,3,"air")]
	[InlineDataAttribute(4,3, "moon air" )]
	[InlineDataAttribute(5,4, "moon air" )]
	[InlineDataAttribute(6,4, "moon air" )]
	[InlineDataAttribute(7,5, "moon air" )]
	public void EnergyTrack(int revealedSpaces, int expectedEnergyGrowth, string elements ) {
		var fixture = new ConfigurableTestFixture { Spirit = new Bringer() };
		fixture.VerifyEnergyTrack(revealedSpaces, expectedEnergyGrowth, elements);
	}

	[Trait("Presence","CardTrack")]
	[Theory]
	[InlineDataAttribute(1,2,"")]
	[InlineDataAttribute(2,2,"")]
	[InlineDataAttribute(3,2,"")]
	[InlineDataAttribute(4,3,"")]
	[InlineDataAttribute(5,3,"")]
	[InlineDataAttribute(6,3,"")]
	public void CardTrack(int revealedSpaces, int expectedCardPlayCount, string elements){
		var fixture = new ConfigurableTestFixture { Spirit = new Bringer() };
		fixture.VerifyCardTrack(revealedSpaces, expectedCardPlayCount, elements);
	}

	void Assert_GainsFirstMinorCard() {
		_spirit.Assert_HasCardAvailable( "Drought" );
	}

}
