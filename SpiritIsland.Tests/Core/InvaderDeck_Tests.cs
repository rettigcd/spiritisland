using Shouldly;
using Xunit;

namespace SpiritIsland.Tests.Core {

	public class InvaderDeck_Tests {

		[Fact]
		public void Memento_RoundTrip() {

			// Given: a deck in some advanced state
			var sut = new InvaderDeck();
			Advance( sut );

			//   And: we have saved the desired state
			var memento = sut.SaveToMemento();
			string expected = TakeSnapShot( sut );

			//   And: advanced beyond the saved state
			Advance( sut );

			//  When: we restore the saved state
			sut.LoadFrom( memento );

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
			return sut.Ravage[0].Text + " : " + sut.Build[0].Text + " : " + sut.Explore[0].Text;
		}
	}

}
