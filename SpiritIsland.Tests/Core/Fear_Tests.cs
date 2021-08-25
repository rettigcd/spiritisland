using SpiritIsland.Basegame;
using SpiritIsland.SinglePlayer;
using Xunit;

namespace SpiritIsland.Tests {

	public class Fear_Tests {

		readonly Spirit spirit;
		readonly GameState gs;
		public Fear_Tests() {
			spirit = new RiverSurges();
			gs = new GameState( spirit, Board.BuildBoardA() );
		}

		[Fact]
		public void TriggerDirect() {
			Given_EnoughFearToTriggerCard();
			_ = gs.ApplyFear(); // When
			Assert_PresentsFearToUser();
		}

		[Fact]
		public void TriggerAsPartofInvaderActions() {
			Given_EnoughFearToTriggerCard();
			_ = new InvaderPhase(gs).ActAsync(); // When
			Assert_PresentsFearToUser();
		}

		void Given_EnoughFearToTriggerCard() {
			gs.AddFearDirect( new FearArgs { count = 4 } );
		}

		void Assert_PresentsFearToUser() {
			spirit.Action.AssertPrompt_ChooseFirst( "Activating Fear" );
		}


	}
}
