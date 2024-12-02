namespace SpiritIsland.Tests.Spirits.River; 

[Collection("BaseGame Spirits")]
public class BoonOfVigor_Tests {

	[Fact]
	public async Task BoonOfVigor_TargetSelf() {

		Spirit river = new RiverSurges();
		GameState gs = new SoloGameState( river, Boards.A );

		await river.When_ResolvingCard<BoonOfVigor>();

		// Then: received 1 energy
		Assert.Equal( 1, river.Energy );

	}

	[Theory]
	[InlineData( 0 )]
	[InlineData( 3 )]
	[InlineData( 10 )]
	public async Task BoonOfVigor_TargetOther( int expectedEnergyBonus ) {

		GameState gs = new GameConfiguration()
			.ConfigSpirits(RiverSurges.Name,LightningsSwiftStrike.Name)
			.ConfigBoards("A","B")
			.BuildShell();

		Spirit river = gs.Spirits[0];


		//  That: purchase N cards
		var otherSpirit = gs.Spirits[1];
		Given_PurchasedFakePowercards(otherSpirit, expectedEnergyBonus);

		//   And: Purchased Boon of Vigor
		PowerCard Card = river.Given_PurchasedCard(BoonOfVigor.Name);
		river.Assert_CardIsReady(Card,Phase.Fast);

		Task t = Card.ActivateAsync( river );

		new VirtualUser(river).TargetsSpirit(BoonOfVigor.Name, "River Surges in Sunlight,[Lightning's Swift Strike]");

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




