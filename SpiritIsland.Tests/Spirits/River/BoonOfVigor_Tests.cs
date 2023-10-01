namespace SpiritIsland.Tests.Spirits.River; 

public class BoonOfVigor_Tests : SpiritCards_Tests {

	public BoonOfVigor_Tests():base( new RiverSurges() ) {
	}

	[Fact]
	public void BoonOfVigor_TargetSelf() {

		Given_GameWithSpirits( _spirit );
		_gameState.Phase = Phase.Fast;

		Given_PurchasedCard( BoonOfVigor.Name );
		Assert_CardIsReady( _card, Phase.Fast );

		When_PlayingCard();

		User.Assert_Done();

		// Then: received 1 energy
		Assert.Equal( 1, _spirit.Energy );

	}

	[Theory]
	[InlineData( 0 )]
	[InlineData( 3 )]
	[InlineData( 10 )]
	public void BoonOfVigor_TargetOther( int expectedEnergyBonus ) {

		Given_GameWithSpirits(_spirit, new LightningsSwiftStrike());
		_gameState.Phase = Phase.Fast;

		//  That: purchase N cards
		var otherSpirit = _gameState.Spirits[1];
		Given_PurchasedFakePowercards(otherSpirit, expectedEnergyBonus);

		//   And: Purchased Boon of Vigor
		Given_PurchasedCard(BoonOfVigor.Name);
		Assert_CardIsReady(_card,Phase.Fast);

		When_PlayingCard();
		
		User.TargetsSpirit(BoonOfVigor.Name, "River Surges in Sunlight,[Lightning's Swift Strike]");

		User.Assert_Done();

		// Then: received 1 energy
		Assert.Equal(expectedEnergyBonus, otherSpirit.Energy);

	}

	[Fact]
	public void BoonOfVigor_Stats() {
		Assert_CardStatus( PowerCard.For<BoonOfVigor>(), 0, Phase.Fast, "sun water plant" );
	}

}




