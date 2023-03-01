namespace SpiritIsland.Tests.Spirits.BringerNS;

public class Bringer_GrowthTests : GrowthTests {

	public Bringer_GrowthTests():base( new Bringer() ) {}

	[Fact] 
	public void ReclaimAll_PowerCard() { // Growth Option 1

		// reclaim, +1 power card
		Given_HalfOfPowercardsPlayed();

		_spirit.When_Growing( 0, () => {
			User.SelectsMinorDeck();
			User.SelectMinorPowerCard();
		} );

		// Then:
		Assert_AllCardsAvailableToPlay( 4 + 1 );
		Assert_HasCardAvailable( "Drought" );

	}

	[Fact] 
	public void Reclaim1_Presence() { // Growth Option 2
		// reclaim 1, add presense range 0
		Given_HalfOfPowercardsPlayed();
		Given_HasPresence( _board[4] );

		_spirit.When_Growing( 1, ()=> {
			User.Growth_Reclaims1( "Predatory Nightmares $2 (Slow),[Dreams of the Dahan $0 (Fast)]" );
			User.Growth_PlacesPresence( "energy>A4" );
		} );

		_spirit.Hand.Count.ShouldBe( 3 );
	}

	[Fact] 
	public void PowerCard_Presence() { // Growth Option 3
		// +1 power card, +1 pressence range 1
		Given_HasPresence( _board[1] );

		_spirit.When_Growing( 2, ()=> {
			User.Growth_DrawsPowerCard();
			User.SelectsMinorDeck();
			User.SelectMinorPowerCard();
			User.Growth_PlacesEnergyPresence( "A1;A2;A4;A5;A6" );
		} );

		Assert_GainsFirstMinorCard();
		Assert_BoardPresenceIs( "A1:2" );
	}

	[Fact] 
	public void PresenseOnPieces_Energy(){ // Growth Option 4

		_board = LineBoard.MakeBoard();
		_gameState = new GameState( _spirit, _board );

		Given_HasPresence(_board[5]);
		_board[6].Tokens.Dahan.Init(1);
		_board[7].Tokens.AdjustDefault( Human.Explorer, 1 );
		_board[8].Tokens.AdjustDefault( Human.Town, 1 );
		_board[0].Tokens.AdjustDefault( Human.City, 1 );

		// add presense range 4 Dahan or Invadors, +2 energy
		_spirit.When_Growing( () => {
			// User.Growth_SelectsOption( "GainEnergy(2) / PlacePresence(4,dahan or invaders)" );
			User.Growth_SelectAction( $"PlacePresence(4,{Target.Dahan}Or{Target.Invaders})" );
			User.Growth_PlacesEnergyPresence( "T6;T7;T8;T9" );
		} );

		Assert.Equal(2,_spirit.EnergyPerTurn);
		Assert_HasEnergy(2+2);
		Assert_BoardPresenceIs("T5:1,T6:1");
	}

	[Trait("Presence","EnergyTrack")]
	[Theory]
	[InlineDataAttribute(1,2,"")]
	[InlineDataAttribute(2,2,"air")]
	[InlineDataAttribute(3,3,"air")]
	[InlineDataAttribute(4,3, "moon air" )]
	[InlineDataAttribute(5,4, "moon air" )]
	[InlineDataAttribute(6,4, "moon air any" )]
	[InlineDataAttribute(7,5, "moon air any" )]
	public async Task EnergyTrack(int revealedSpaces, int expectedEnergyGrowth, string elements ) {// !!! still async
		var fixture = new ConfigurableTestFixture { Spirit = new Bringer() };
		await fixture.VerifyEnergyTrack(revealedSpaces, expectedEnergyGrowth, elements);
	}

	[Trait("Presence","CardTrack")]
	[Theory]
	[InlineDataAttribute(1,2,"")]
	[InlineDataAttribute(2,2,"")]
	[InlineDataAttribute(3,2,"")]
	[InlineDataAttribute(4,3,"")]
	[InlineDataAttribute(5,3,"")]
	[InlineDataAttribute(6,3,"any")]
	public async Task CardTrack(int revealedSpaces, int expectedCardPlayCount, string elements){
		var fixture = new ConfigurableTestFixture { Spirit = new Bringer() };
		await fixture.VerifyCardTrack(revealedSpaces, expectedCardPlayCount, elements);
	}

	void Assert_GainsFirstMinorCard() {
		Assert_HasCardAvailable( "Drought" );
	}

}
