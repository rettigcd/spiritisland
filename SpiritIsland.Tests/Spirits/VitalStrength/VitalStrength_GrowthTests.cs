namespace SpiritIsland.Tests.Spirits.VitalStrengthNS;

// https://xunit.net/docs/running-tests-in-parallel#parallelism-in-test-frameworks
// By Default all tests in the same class are in a Collection and will not run in parallel
// Collection("xxx") creates larger collections and therefore fewer of them, so there are fewer running in parallel

[Collection("BaseGame Spirits")]
public class VitalStrength_GrowthTests : BoardAGame {

	public VitalStrength_GrowthTests()
		:base( new VitalStrength() ){}

	[Fact]
	public async Task ReclaimAndPresence() {
		// (A) reclaim, +1 presense range 2
		_spirit.Given_HalfOfHandDiscarded();
		_spirit.Given_IsOn( _board[3] );

		await _spirit.When_Growing( (user) => {
			user.SelectsGrowthA_Reclaim_PP2();
		} );

		_spirit.Assert_AllCardsAvailableToPlay();

	}

	[Fact]
	public async Task PowercardAndPresence() {
		// (B) +1 power card, +1 presense range 0
		_spirit.Given_IsOn( _board[4] );

		await _spirit.When_Growing( (user) => {
			user.SelectsGrowthB_DrawCard_PP0();
		} );

		Assert.Equal( 5, _spirit.Hand.Count );
	}

	[Fact]
	public async Task PresenseAndEnergy() {
		// (C) +1 presence range 1, +2 energy
		_spirit.Given_IsOn( _board[1] );

		await _spirit.When_Growing((user) => { 
			user.SelectsGrowthC_Energy_PP1(); 
		});

		Assert.Equal( 3, _spirit.EnergyPerTurn );
		_spirit.Assert_HasEnergy( 3 + 2 );
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
		return new VitalStrength().VerifyEnergyTrack( revealedSpaces, expectedEnergyGrowth, "" );
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
		return new VitalStrength().VerifyCardTrack(revealedSpaces,expectedCardPlayCount,"");
	}

}

