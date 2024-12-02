using SpiritIsland.Log;

namespace SpiritIsland.Tests.Fear;

public class Depopulation_Tests {

	#region constructor

	public Depopulation_Tests() {
		_spirit = new LightningsSwiftStrike();
		// _user = new VirtualUser( _spirit );
		_board = Boards.A;
		_gameState = new SoloGameState( _spirit, _board );
		_gameState.DisableInvaderDeck();
		_fearCard = _gameState.WatchForFearCard();
		_gameState.Initialize();
		_gameState.Fear.Deck.Pop();
		_gameState.Fear.ActivatedCards.Push( new Depopulation() );
	}

	readonly Task<FearCardRevealed> _fearCard;

	#endregion

	[Trait( "Feature", "Downgrade" )]
	[Fact]
	public async Task Level3_DowngradesCity() {

		var space = _board[8];

		// Given: space has City Only
		space.ScopeSpace.InitDefault(Human.City,1);
		space.ScopeSpace.InitDefault( Human.Town, 0 );
		space.ScopeSpace.InvaderSummary().ShouldBe( "1C@3" );

		// Given: Terror level is 3
		Given_FearLevelIs(3);

		// When: Doing Invader phase (fear+ragage)
		await _gameState.Fear.ResolveActivatedCards()
			.AwaitUser( async u => { 
				// u.NextDecision.HasPrompt( "Activating Fear" ).HasOptions(  ).Choose( "Depopulation" );
				(await _fearCard).Card.Text.ShouldBe("Depopulation");
				u.NextDecision.HasPrompt( "Select token for Remove 1 Town, or Replace 1 City with 1 Town" ).HasOptions( "C@3 on A2,C@3 on A8" ).Choose( "C@3 on A8" );
			} )
			.ShouldComplete( "fear" );

		// Then: all dahan killed
		space.ScopeSpace.InvaderSummary().ShouldBe("1T@2");
	}

	void Given_FearLevelIs(int desiredFearLevel ) {
		var fear = _gameState.Fear;
		var savedTop = fear.Deck.Pop();
		while(fear.TerrorLevel < desiredFearLevel)
			fear.Deck.Pop();
		fear.Deck.Pop(); // pop 1 extra so we can restore the saved
		fear.Deck.Push(savedTop);
	}

	readonly GameState _gameState;
	readonly Spirit _spirit;
	// readonly VirtualUser _user;
	readonly Board _board;

}