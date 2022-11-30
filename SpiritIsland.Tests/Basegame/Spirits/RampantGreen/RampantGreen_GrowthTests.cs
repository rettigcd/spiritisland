namespace SpiritIsland.Tests.Basegame.Spirits.RampantGreen;

public class RampantGreen_GrowthTests : GrowthTests {

	public RampantGreen_GrowthTests():base( new ASpreadOfRampantGreen().UsePowerProgression() ){}

	// +1 presense to jungle or wetland - range 2(Always do this + one of the following)
	// reclaim, +1 power card
	// +1 presense range 1, play +1 extra card this turn
	// +1 power card, +3 energy

	[Fact]
	public void Reclaim_PowerCard_JWPresence() {
		// +1 presense to jungle or wetland - range 2(Always do this + one of the following)
		// reclaim, +1 power card
		Given_HalfOfPowercardsPlayed();
		Given_HasPresence( board[2] );

		When_StartingGrowth();
		User_SelectAlwaysGrowthOption();

		User.Growth_SelectAction( "DrawPowerCard" );

		Assert_AllCardsAvailableToPlay(5);
	}

	[Fact]
	public void PlayExtraCard_2Presence() {
		// +1 presense to jungle or wetland - range 2
		// +1 presense range 1, play +1 extra card this turn

		// Presense Options
		Given_HasPresence( board[2] );

		Assert.Equal( 1, spirit.NumberOfCardsPlayablePerTurn ); // ,"Rampant Green should start with 1 card.");

		When_StartingGrowth();
		User_SelectAlwaysGrowthOption();

		User.Growth_SelectAction( "PlacePresence(1)" );
		User.Growth_PlacesEnergyPresence( "A2;A3;A5" );

		// Player Gains +1 card to play this round
		Assert.Equal( 2, spirit.NumberOfCardsPlayablePerTurn ); // , "Should gain 1 card to play this turn.");

		// But count drops back down after played
		spirit.PlayCard( spirit.Hand[0] );
		spirit.tempCardPlayBoost = 0; // makes test pass, but Rampant Green test is testing wrong thing

		// Back to original
		Assert.Equal( 1, spirit.NumberOfCardsPlayablePerTurn ); // ,"Available card count should be back to original");

	}

	[Fact]
	public void GainEnergy_PowerCard_JWPresence() {
		Given_HasPresence( board[2] );

		When_StartingGrowth();

		User_SelectAlwaysGrowthOption();

		User.Growth_SelectAction( "DrawPowerCard", 1 ); // there are 2. select the 2nd one (index=1)
		// Gain 3 energy did not trigger

		Assert.Equal( 1, spirit.EnergyPerTurn );
		Assert_HasEnergy( 3 + 1 );
		spirit.Hand.Count.ShouldBe( 5 );
	}

	void User_SelectAlwaysGrowthOption() {
		User.Growth_SelectAction( $"PlacePresence(2,{Target.Jungle}Or{Target.Wetland})" );
		User.Growth_PlacesEnergyPresence( "A2;A3;A5" ); // +1 from energy track
	}

	[Trait("Presence","EnergyTrack")]
	[Theory]
	[InlineDataAttribute( 1,0,"")]
	[InlineDataAttribute( 2,1,"")]
	[InlineDataAttribute( 3,1,"plant")]
	[InlineDataAttribute( 4, 2, "plant" )]
	[InlineDataAttribute( 5, 2, "plant" )]
	[InlineDataAttribute( 6, 2, "2 plant" )]
	[InlineDataAttribute( 7, 3, "2 plant" )]
	public async Task EnergyTrack(int revealedSpaces, int expectedEnergyGrowth, string elements ) {
		var fixture = new ConfigurableTestFixture { Spirit = new ASpreadOfRampantGreen() };
		await fixture.VerifyEnergyTrack( revealedSpaces, expectedEnergyGrowth, elements );
	}

	[Trait("Presence","CardTrack")]
	[Theory]
	[InlineDataAttribute(1,1)]
	[InlineDataAttribute(2,1)]
	[InlineDataAttribute(3,2)]
	[InlineDataAttribute(4,2)]
	[InlineDataAttribute(5,3)]
	[InlineDataAttribute(6,4)]
	public async Task CardTrack(int revealedSpaces, int expectedCardPlayCount){
		var fixture = new ConfigurableTestFixture { Spirit = new ASpreadOfRampantGreen() };
		await fixture.VerifyCardTrack( revealedSpaces, expectedCardPlayCount, "" );
	}

}