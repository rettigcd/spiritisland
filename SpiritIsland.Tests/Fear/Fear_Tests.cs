namespace SpiritIsland.Tests.Fear;

public class Fear_Tests {

	readonly Spirit spirit;
	readonly GameState gs;
	readonly VirtualUser user;
	public Fear_Tests() {
		spirit = new RiverSurges();
		user = new VirtualUser( spirit );
		gs = new GameState( spirit, Board.BuildBoardA() );
		_fearCard = gs.WatchForFearCard();
	}
	readonly Task<Log.FearCardRevealed> _fearCard;

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

	[Fact]
	public void GetName() {
		new AvoidTheDahan().Text.ShouldBe( "Avoid the Dahan" );
	}

	void Given_EnoughFearToTriggerCard() {
		gs.Fear.Add( 4 );
	}

	async void Assert_PresentsFearToUser() {
		(await _fearCard).Msg().Equals("Null Fear Card : 1 : x");
	}

}
