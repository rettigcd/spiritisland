namespace SpiritIsland.Tests.Fear;

[Trait( "Invaders", "Ravage" )]
public class BeliefTakesRoot_Tests {

	#region constructor

	public BeliefTakesRoot_Tests() {
		_gameState = new SoloGameState();
		_spirit = _gameState.Spirit;
		_gameState.DisableInvaderDeck();
		_gameState.Initialize(); 

		_invaderCard = InvaderDeckBuilder.Level1Cards[0];
		_ravageSpace = _gameState.Island.Boards[0].Spaces
			.Select(s=>_gameState.Tokens[s])
			.Where( ((InvaderCard)_invaderCard).MatchesCard )
			.First();
	}

	#endregion

	[Fact]
	public async Task NormalRavage() {

		Given_DahanAndTownsInSpaceWithPresence(10,1);

		await _invaderCard.When_Ravaging();

		// Then: all dahan killed
		_ravageSpace.Dahan.CountAll.ShouldBe(0);
		_ravageSpace.Blight.Any.ShouldBe(true);
	}

	[Fact]
	public async Task Level1_NoBlightDahanLives() {
		Given_DahanAndTownsInSpaceWithPresence(1, 1);

		// Given: Believe Takes Root
		await new BeliefTakesRoot().ActAsync(1);

		await _invaderCard.When_Ravaging();

		// Then: 1 dahan left
		Assert.Equal( 1, _ravageSpace.Dahan.CountAll );

		//   And: 0 towns
		_ravageSpace.Assert_HasInvaders("");
		Assert.False( _ravageSpace.Blight.Any );

	}

	[Fact]
	public async Task Level1_DefendNotMoreThan2() { // not more th
		Given_DahanAndTownsInSpaceWithPresence(2, 5);

		// Given:
		await new BeliefTakesRoot().ActAsync(1);

		await _invaderCard.When_Ravaging();

		// Then: 1 dahan left
		Assert.Equal( 1, _ravageSpace.Dahan.CountAll );

		//   And: 0 towns
		_ravageSpace.Assert_HasInvaders( "1T@2" );
		Assert.True( _ravageSpace.Blight.Any );
	}

	void Given_DahanAndTownsInSpaceWithPresence(int desiredCount,int presenceCount) { 
		// Add: dahan
		_ravageSpace.Dahan.Init( desiredCount );
		// Add towns
		_ravageSpace.AdjustDefault( Human.Town, desiredCount );

		//   And: Presence
		while(presenceCount-->0)
			_spirit.Given_IsOn(_ravageSpace);
	}

	readonly SoloGameState _gameState;
	readonly InvaderCard _invaderCard;
	readonly Space _ravageSpace;
	readonly Spirit _spirit;

}