namespace SpiritIsland.Tests.Fear;

public class Fear_Tests {

	readonly GameState _gs;

	public Fear_Tests() {
		_gs = new SoloGameState();
		_fearCard = _gs.WatchForFearCard();
	}
	readonly Task<Log.FearCardRevealed> _fearCard;

	[Fact]
	public async Task TriggerDirect() {
		Given_EnoughFearToTriggerCard();
		await _gs.Fear.ResolveActivatedCards(); // When
		Assert_PresentsFearToUser();
	}

	[Fact]
	public async Task TriggerAsPartofInvaderActions() {
		Given_EnoughFearToTriggerCard();
		await InvaderPhase.ActAsync( _gs ); // When
		Assert_PresentsFearToUser();
	}

	[Fact]
	public void GetName() {
		new AvoidTheDahan().Text.ShouldBe( "Avoid the Dahan" );
	}

	void Given_EnoughFearToTriggerCard() {
		_gs.Fear.Add( 4 );
	}

	async void Assert_PresentsFearToUser() {
		(await _fearCard).Msg().Equals("Null Fear Card : 1 : x");
	}

}
