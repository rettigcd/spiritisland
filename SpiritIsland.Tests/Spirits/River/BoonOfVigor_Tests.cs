namespace SpiritIsland.Tests.Spirits.River; 

[Collection("BaseGame Spirits")]
public class BoonOfVigor_Tests {

	GameState GameState;
	PowerCard Card;
	readonly Spirit Spirit;
	readonly VirtualUser User;

	public BoonOfVigor_Tests() {
		Spirit = new RiverSurges();
		User = new VirtualUser( Spirit );
	}

	[Fact]
	public async Task BoonOfVigor_TargetSelf() {

		GameState = new GameState( Spirit, Board.BuildBoardA() );
		GameState.Phase = Phase.Fast;

		Card = Spirit.Given_PurchasedCard( BoonOfVigor.Name );
		Spirit.Assert_CardIsReady( Card, Phase.Fast );

		await Card.ActivateAsync( Spirit ).ShouldComplete();

		User.Assert_Done();

		// Then: received 1 energy
		Assert.Equal( 1, Spirit.Energy );

	}

	[Theory]
	[InlineData( 0 )]
	[InlineData( 3 )]
	[InlineData( 10 )]
	public async Task BoonOfVigor_TargetOther( int expectedEnergyBonus ) {

		GameState = new GameState(
			new Spirit[]{ Spirit, new LightningsSwiftStrike() },
			new Board[] { 
				Board.BuildBoardA(),
				Board.BuildBoardB( GameBuilder.TwoBoardLayout[1] )
			}
		);

		GameState.Phase = Phase.Fast;

		//  That: purchase N cards
		var otherSpirit = GameState.Spirits[1];
		Given_PurchasedFakePowercards(otherSpirit, expectedEnergyBonus);

		//   And: Purchased Boon of Vigor
		Card = Spirit.Given_PurchasedCard(BoonOfVigor.Name);
		Spirit.Assert_CardIsReady(Card,Phase.Fast);

		Task t = Card.ActivateAsync( Spirit );

		
		User.TargetsSpirit(BoonOfVigor.Name, "River Surges in Sunlight,[Lightning's Swift Strike]");

		User.Assert_Done();

		await t.ShouldComplete();

		// Then: received 1 energy
		Assert.Equal(expectedEnergyBonus, otherSpirit.Energy);

	}

	static void Given_PurchasedFakePowercards(Spirit otherSpirit, int expectedEnergyBonus) {
		for (int i = 0; i < expectedEnergyBonus; ++i) {
			var otherCard = PowerCard.For(typeof(SpiritIsland.Basegame.GiftOfLivingEnergy));
			otherSpirit.InPlay.Add(otherCard);
			otherSpirit.AddActionFactory(otherCard);
		}
	}


	[Fact]
	public void BoonOfVigor_Stats() {
		PowerCard.For(typeof(BoonOfVigor)).Assert_CardStatus( 0, Phase.Fast, "sun water plant" );
	}

}




