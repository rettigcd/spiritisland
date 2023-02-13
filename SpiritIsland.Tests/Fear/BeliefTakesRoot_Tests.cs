namespace SpiritIsland.Tests.Fear;

[Trait( "Invaders", "Ravage" )]
public class BeliefTakesRoot_Tests {

	#region constructor

	public BeliefTakesRoot_Tests() {
		spirit = new LightningsSwiftStrike();
		User = new VirtualUser( spirit );
		gameState = new GameState( spirit, Board.BuildBoardA() );
		gameState.DisableInvaderDeck();
		gameState.Initialize(); 
		gameState.Fear.Deck.Pop();
		gameState.Fear.PushOntoDeck( new BeliefTakesRoot() );

		invaderCard = InvaderDeckBuilder.Level1Cards[0];
		ravageSpace = gameState.Island.Boards[0].Spaces.Where( ((InvaderCard)invaderCard).MatchesCard ).First();
	}

	#endregion

	[Fact]
	public void NullFearCard_NormalRavage() {

		gameState.Fear.Deck.Pop();
		gameState.Fear.PushOntoDeck( new NullFearCard() );

		Given_DahanAndTownsInSpaceWithPresence(10,1);

		_ = When_AddFearApplyFearAndRavage();
		User.AcknowledgesFearCard("Null Fear Card : 1 : x");

		// Then: all dahan killed
		ravageSpace.Tokens.Dahan.CountAll.ShouldBe(0);
		gameState.Tokens[ravageSpace].Blight.Any.ShouldBe(true);
	}

	[Fact]
	public void Level1_NoBlightDahanLives() {
		Given_DahanAndTownsInSpaceWithPresence(1,1);

		_ =  When_AddFearApplyFearAndRavage();

		User.AcknowledgesFearCard( FearCardAction );

		// Then: 1 dahan left
		Assert.Equal( 1, ravageSpace.Tokens.Dahan.CountAll );

		//   And: 0 towns
		gameState.Assert_Invaders(ravageSpace,"");
		Assert.False( gameState.Tokens[ ravageSpace ].Blight.Any );

	}

	[Fact]
	public void Level1_DefendNotMoreThan2() { // not more th
		Given_DahanAndTownsInSpaceWithPresence( 2, 5 );

		_ = When_AddFearApplyFearAndRavage();

		User.AcknowledgesFearCard( FearCardAction );

		// Then: 1 dahan left
		Assert.Equal( 1, ravageSpace.Tokens.Dahan.CountAll );

		//   And: 0 towns
		gameState.Assert_Invaders(ravageSpace, "1T@2" );
		Assert.True( gameState.Tokens[ ravageSpace ].Blight.Any );
	}

	void Given_DahanAndTownsInSpaceWithPresence(int desiredCount,int presenceCount) { 
		// Add: dahan
		ravageSpace.Tokens.Dahan.Init( desiredCount );
		// Add towns
		gameState.Tokens[ravageSpace].AdjustDefault( Human.Town, desiredCount );

		//   And: Presence
		while(presenceCount-->0)
			spirit.Presence.PlaceOn(ravageSpace, gameState).Wait();
	}

	async Task When_AddFearApplyFearAndRavage() {
		gameState.Fear.AddDirect( new FearArgs( 4 ) );
		await gameState.Fear.Apply();
		await new RavageSlot().ActivateCard( invaderCard, gameState );
	}

	const string FearCardAction = "Belief takes Root : 1 : Defend 2 in all lands with Presence.";
	readonly GameState gameState;
	readonly InvaderCard invaderCard;
	readonly Space ravageSpace;
	readonly Spirit spirit;
	readonly VirtualUser User;

}