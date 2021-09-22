using Shouldly;
using SpiritIsland.Basegame;
using Xunit;

namespace SpiritIsland.Tests {

	public class FearImage_Tests {

		[Fact]
		public void GetName() {
			new DisplayFearCard( new AvoidTheDahan() )
				.Text.ShouldBe( "Avoid the Dahan" );
		}

		[Fact]
		public void LoadImages() { // !!!

			//IFearOptions[] baseGameFearCards = new IFearOptions[] {
			//	new AvoidTheDahan(),
			//	new BeliefTakesRoot(),
			//	new DahanEnheartened(),
			//	new DahanOnTheirGuard(),
			//	new DahanRaid(),
			//	new EmigrationAccelerates(),
			//	new FearOfTheUnseen(),
			//	new Isolation(),
			//	new OverseasTradeSeemSafer(),
			//	new Retreat(),
			//	new Scapegoats(),
			//	new SeekSafety(),
			//	new TallTalesOfSavagery(),
			//	new TradeSuffers(),
			//	new WaryOfTheInterior()
			//};

//			var mgr = new FearCardImageManager();

			//foreach(var card in baseGameFearCards) {
			//	string name = new FearCard { FearOptions = card };
			//}

		}

	}

}
