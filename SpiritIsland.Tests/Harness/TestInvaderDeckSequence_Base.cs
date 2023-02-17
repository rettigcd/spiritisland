using SpiritIsland.Log;

namespace SpiritIsland.Tests;

public class TestInvaderDeckSequence_Base {

	protected TestInvaderDeckSequence_Base() {
		var powerCard = PowerCard.For<CallToTend>();

		(_user, _ctx) = TestSpirit.StartGame(powerCard);

		_log = GameState.Current.LogInvaderActions();
		_log.Clear(); // skip over initial Explorer setup
	}

	protected VirtualTestUser _user;
	protected SelfCtx _ctx;
	protected Queue<string> _log;

	protected void GrowAndBuyNoCards() {
		_ctx.ClearAllBlight();
		_user.GrowAndBuyNoCards();
	}

}

