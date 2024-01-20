internal static class Misc_Extensions {
	internal static void Assert_CardStatus( this PowerCard card, int expectedCost, Phase expectedSpeed, string expectedElements ) {
		Assert.Equal( expectedCost, card.Cost );
		Assert.Equal( expectedSpeed, card.DisplaySpeed );

		var cardElements = card.Elements.BuildElementString(false);
		Assert.Equal( expectedElements, string.Join("",cardElements));

	}

}