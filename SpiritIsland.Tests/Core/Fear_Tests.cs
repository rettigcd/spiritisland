using Shouldly;
using SpiritIsland.Basegame;
using SpiritIsland.SinglePlayer;
using Xunit;

namespace SpiritIsland.Tests.Core {

	public class Fear_Tests {

		readonly Spirit spirit;
		readonly GameState gs;
		readonly VirtualUser user;
		public Fear_Tests() {
			spirit = new RiverSurges();
			user = new VirtualUser( spirit );
			gs = new GameState( spirit, Board.BuildBoardA() );
		}

		[Fact]
		public void TriggerDirect() {
			Given_EnoughFearToTriggerCard();
			_ = gs.Fear.Apply(); // When
			Assert_PresentsFearToUser();
		}

		[Fact]
		public void TriggerAsPartofInvaderActions() {
			Given_EnoughFearToTriggerCard();
			_ = InvaderPhase.ActAsync( gs ); // When
			Assert_PresentsFearToUser();
		}

		void Given_EnoughFearToTriggerCard() {
			gs.Fear.AddDirect( new FearArgs { count = 4 } );
		}

		void Assert_PresentsFearToUser() {
			user.AcknowledgesFearCard("Null Fear Card : 1 : x");
		}

		[Fact]
		public void GetName() {
			new AvoidTheDahan().Text.ShouldBe( "Avoid the Dahan" );
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
