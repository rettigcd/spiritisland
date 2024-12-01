namespace SpiritIsland.Tests.Spirits.ShadowsNS;

[Collection("BaseGame Spirits")]
public class ShadowsFlicker_GrowthTests : BoardAGame {

	public ShadowsFlicker_GrowthTests():base( new Shadows() ){
	}

	[Fact]
	public async Task Reclaim_PowerCard(){
		// reclaim, gain power Card
		_spirit.Given_HalfOfHandDiscarded();

		await _spirit.When_Growing( 0, (user) => {
			user.SelectsMinorDeck();
			user.SelectMinorPowerCard();
		} );

		Assert.Equal(5,_spirit.Hand.Count); // drew 1 card
	}

	[Fact]
	public async Task PowerAndPresence(){
		// gain power card, add a presense range 1
		_spirit.Given_IsOn( _board[1] );

		await _spirit.When_Growing( 1, (user) => {
			user.Growth_DrawsPowerCard();
			user.SelectsMinorDeck();
			user.SelectMinorPowerCard();

			user.Growth_PlacesEnergyPresence( "A1;A2;A4;A5;A6" );
		} );

		Assert.Equal(5,_spirit.Hand.Count); // drew 1 card
	}

	[Fact]
	public async Task PresenceAndEnergy(){
		// add a presence within 3, +3 energy
		_spirit.Given_IsOn( _board[3] );

		await _spirit.When_Growing( (user) => {
			//		User.Growth_SelectsOption( "PlacePresence(3) / GainEnergy(3)" );
			User.Growth_SelectAction( "Place Presence(3)" );
			User.Growth_PlacesEnergyPresence( "A1;A2;A3;A4;A5;A6;A7;A8" );
		} );

		_spirit.Assert_HasEnergy(3+1); // 1 from energy track
	}

	[Trait("Presence","EnergyTrack")]
	[Theory]
	[InlineDataAttribute(1,0)]
	[InlineDataAttribute(2,1)]
	[InlineDataAttribute(3,3)]
	[InlineDataAttribute(4,4)]
	[InlineDataAttribute(5,5)]
	[InlineDataAttribute(6,6)]
	public Task EnergyTrack(int revealedSpaces, int expectedEnergyGrowth ){
		var fix = new ConfigurableTestFixture { Spirit = new Shadows() };
		return fix.VerifyEnergyTrack( revealedSpaces, expectedEnergyGrowth, "" );
	}

	[Trait("Presence","CardTrack")]
	[Theory]
	[InlineDataAttribute(1,1)]
	[InlineDataAttribute(2,2)]
	[InlineDataAttribute(3,3)]
	[InlineDataAttribute(4,3)]
	[InlineDataAttribute(5,4)]
	[InlineDataAttribute(6,5)]
	public Task CardTrack(int revealedSpaces, int expectedCardPlayCount){
		var fix = new ConfigurableTestFixture { Spirit = new Shadows() };
		return fix.VerifyCardTrack(revealedSpaces, expectedCardPlayCount, "");
	}

}
