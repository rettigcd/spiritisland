namespace SpiritIsland.Tests.Fear;

public class DahanOnTheirGuard_Tests {

	#region constructor

	public DahanOnTheirGuard_Tests() {
		spirit = new LightningsSwiftStrike();
		gameState = new GameState( spirit, Board.BuildBoardA() );
		gameState.DisableInvaderDeck();
		gameState.Initialize();
		gameState.Fear.Deck.Pop();
		gameState.Fear.PushOntoDeck( new DahanOnTheirGuard() );

		invaderCard = InvaderDeckBuilder.Level1Cards[0];
		ravageSpace = gameState.Island.Boards[0].Spaces.Where( ((InvaderCard)invaderCard).MatchesCard ).First();
	}

	#endregion

	[Trait( "Feature", "Defend" )]
	[Fact]
	public async Task NoFearCard_NormalRavage() {

		// Disable destroying presence
		gameState.DisableBlightEffect();

		Given_DahanAndTowns( 2, 2 );

		// When: Doing Invader phase (fear+ragage)
		await gameState.Fear.Apply().ShouldComplete( "fear");
		await invaderCard.When_Ravaging();

		// Then: all dahan killed
		Assert.Equal( 0, ravageSpace.Tokens.Dahan.CountAll );
		Assert.True( gameState.Tokens[ ravageSpace ].Blight.Any );
	}

	[Trait( "Feature", "Defend" )]
	[Fact]
	public async Task Level1_DefendOnly1AndNotMorePerDahan() { // not more th
		Given_DahanAndTowns( 4, 4 );
		// 4 dahan should defend 4

		// When: Doing Invader phase (fear+ragage)

		gameState.Fear.Add( 4 );
		await gameState.Fear.Apply().AwaitUser( (user) => {
			user.AcknowledgesFearCard( "Dahan on their Guard : 1 : In each land, Defend 1 per Dahan." );
		} ).ShouldComplete(DahanOnTheirGuard.Name);

		await invaderCard.When_Ravaging();

		// Then: 0 dahan left
		ravageSpace.Tokens.Dahan.CountAll.ShouldBe( 2 );

		//   And: 2 towns
		ravageSpace.Assert_HasInvaders( "2T@2" );
		gameState.Tokens[ ravageSpace ].Blight.Any.ShouldBe( true );

	}

	void Given_DahanAndTowns( int desiredDahan, int desiredTown ) {
		ravageSpace.Tokens.Dahan.Init( desiredDahan );
		Assert.Equal(desiredDahan,ravageSpace.Tokens.Dahan.CountAll);

		gameState.Tokens[ravageSpace].AdjustDefault( Human.Town, desiredTown );
	}

	readonly GameState gameState;
	readonly InvaderCard invaderCard;
	readonly Space ravageSpace;
	readonly Spirit spirit;

}
