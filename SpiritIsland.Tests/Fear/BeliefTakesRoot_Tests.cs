namespace SpiritIsland.Tests.Fear;

[Trait( "Invaders", "Ravage" )]
public class BeliefTakesRoot_Tests {

	#region constructor

	public BeliefTakesRoot_Tests() {
		_spirit = new LightningsSwiftStrike();
		User = new VirtualUser( _spirit );
		_gameState = new GameState( _spirit, Board.BuildBoardA() );
		_gameState.DisableInvaderDeck();
		_gameState.Initialize(); 
		_gameState.Fear.Deck.Pop();
		_gameState.Fear.PushOntoDeck( new BeliefTakesRoot() );

		_invaderCard = InvaderDeckBuilder.Level1Cards[0];
		_ravageSpace = _gameState.Island.Boards[0].Spaces.Where( ((InvaderCard)_invaderCard).MatchesCard ).First();
	}

	#endregion

	[Fact]
	public async Task NullFearCard_NormalRavage() {

		_gameState.Fear.Deck.Pop();
		_gameState.Fear.PushOntoDeck( new NullFearCard() );

		Given_DahanAndTownsInSpaceWithPresence(10,1);

		await When_AddFearApplyFear( () => {
			User.AcknowledgesFearCard( "Null Fear Card : 1 : x" );
		} );
		await _invaderCard.When_Ravaging();

		// Then: all dahan killed
		_ravageSpace.Tokens.Dahan.CountAll.ShouldBe(0);
		_gameState.Tokens[_ravageSpace].Blight.Any.ShouldBe(true);
	}

	[Fact]
	public async Task Level1_NoBlightDahanLives() {
		Given_DahanAndTownsInSpaceWithPresence(1, 1);

		await When_AddFearApplyFear( ()=> {
			User.AcknowledgesFearCard( FearCardAction );
		} );
		await _invaderCard.When_Ravaging();

		// Then: 1 dahan left
		Assert.Equal( 1, _ravageSpace.Tokens.Dahan.CountAll );

		//   And: 0 towns
		_ravageSpace.Assert_HasInvaders("");
		Assert.False( _gameState.Tokens[ _ravageSpace ].Blight.Any );

	}

	[Fact]
	public async Task Level1_DefendNotMoreThan2() { // not more th
		Given_DahanAndTownsInSpaceWithPresence(2, 5);

		await When_AddFearApplyFear( () => {
			User.AcknowledgesFearCard( FearCardAction );
		} );
		await _invaderCard.When_Ravaging();

		// Then: 1 dahan left
		Assert.Equal( 1, _ravageSpace.Tokens.Dahan.CountAll );

		//   And: 0 towns
		_ravageSpace.Assert_HasInvaders( "1T@2" );
		Assert.True( _gameState.Tokens[ _ravageSpace ].Blight.Any );
	}

	void Given_DahanAndTownsInSpaceWithPresence(int desiredCount,int presenceCount) { 
		// Add: dahan
		_ravageSpace.Tokens.Dahan.Init( desiredCount );
		// Add towns
		_gameState.Tokens[_ravageSpace].AdjustDefault( Human.Town, desiredCount );

		//   And: Presence
		while(presenceCount-->0)
			_spirit.Given_IsOn(_ravageSpace);
	}

	async Task When_AddFearApplyFear(Action userActions) {
		_gameState.Fear.Add( 4 );
		await _gameState.Fear.Apply().AwaitUserToComplete( "Fear", userActions );
	}

	const string FearCardAction = "Belief takes Root : 1 : Defend 2 in all lands with Presence.";
	readonly GameState _gameState;
	readonly InvaderCard _invaderCard;
	readonly Space _ravageSpace;
	readonly Spirit _spirit;
	readonly VirtualUser User;

}