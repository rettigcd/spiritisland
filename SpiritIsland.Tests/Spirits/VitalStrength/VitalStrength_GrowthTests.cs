namespace SpiritIsland.Tests.Spirits.VitalStrengthNS;

public class VitalStrength_GrowthTests : GrowthTests {

	readonly new VirtualEarthUser User;

	public VitalStrength_GrowthTests()
		:base( new VitalStrength() ){
		User = new VirtualEarthUser( spirit );
	}

	[Fact]
	public void ReclaimAndPresence() {
		// (A) reclaim, +1 presense range 2
		Given_HalfOfPowercardsPlayed();
		Given_HasPresence( board[3] );

		When_StartingGrowth();

		User.SelectsGrowthA_Reclaim_PP2();

		this.Assert_AllCardsAvailableToPlay();

	}

	[Fact]
	public void PowercardAndPresence() {
		// (B) +1 power card, +1 presense range 0
		Given_HasPresence( board[4] );

		When_StartingGrowth();

		User.SelectsGrowthB_DrawCard_PP0();

		Assert.Equal( 5, spirit.Hand.Count );
	}

	[Fact]
	public void PresenseAndEnergy() {
		// (C) +1 presence range 1, +2 energy
		Given_HasPresence( board[1] );

		When_StartingGrowth();

		User.SelectsGrowthC_Energy_PP1();

		Assert.Equal( 3, spirit.EnergyPerTurn );
		Assert_HasEnergy( 3 + 2 );
	}

	[Trait("Presence","EnergyTrack")]
	[Theory]
	[InlineDataAttribute(1,2)]
	[InlineDataAttribute(2,3)]
	[InlineDataAttribute(3,4)]
	[InlineDataAttribute(4,6)]
	[InlineDataAttribute(5,7)]
	[InlineDataAttribute(6,8)]
	public Task EnergyTrack(int revealedSpaces, int expectedEnergyGrowth) {
		var fix = new ConfigurableTestFixture { Spirit = new VitalStrength() };
		return fix.VerifyEnergyTrack( revealedSpaces, expectedEnergyGrowth, "" );
	}

	[Trait("Presence","CardTrack")]
	[Theory]
	[InlineDataAttribute(1,1)]
	[InlineDataAttribute(2,1)]
	[InlineDataAttribute(3,2)]
	[InlineDataAttribute(4,2)]
	[InlineDataAttribute(5,3)]
	[InlineDataAttribute(6,4)]
	public Task CardTrack(int revealedSpaces, int expectedCardPlayCount){
		var fix = new ConfigurableTestFixture { Spirit = new VitalStrength() };
		return fix.VerifyCardTrack(revealedSpaces,expectedCardPlayCount,"");
	}

}

