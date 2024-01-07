namespace SpiritIsland.Tests.Core;

public class InvaderDeck_Tests {

	[Trait( "Invaders", "Deck" )]
	[Fact]
	public void Memento_RoundTrip() {

		// Given: a deck in some advanced state
		var sut = InvaderDeckBuilder.Default.Build();
		Advance( sut );

		//   And: we have saved the desired state
		var memento = sut.Memento;
		string expected = TakeSnapShot( sut );

		//   And: advanced beyond the saved state
		Advance( sut );

		//  When: we restore the saved state
		sut.Memento = memento;

		// Then: we should get back the expted state
		TakeSnapShot( sut ).ShouldBe( expected );

	}

	static void Advance( InvaderDeck sut ) {
		sut.Advance();
		sut.Advance();
		sut.Advance();
	}

	static string TakeSnapShot( InvaderDeck sut ) {
		//   And: record cards
		return sut.Ravage.Cards[0].Text + " : " + sut.Build.Cards[0].Text + " : " + sut.Explore.Cards[0].Text;
	}

}