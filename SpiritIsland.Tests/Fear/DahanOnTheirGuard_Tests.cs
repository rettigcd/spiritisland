namespace SpiritIsland.Tests.Fear;

public class DahanOnTheirGuard_Tests {

	#region constructor

	public DahanOnTheirGuard_Tests() {
		spirit = new LightningsSwiftStrike();
		gameState = new SoloGameState( spirit,	Boards.A );
		gameState.DisableInvaderDeck();
		gameState.Initialize();

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
		await gameState.Fear.ResolveActivatedCards().ShouldComplete( "fear");
		await invaderCard.When_Ravaging();

		// Then: all dahan killed
		Assert.Equal( 0, ravageSpace.ScopeSpace.Dahan.CountAll );
		Assert.True( gameState.Tokens[ ravageSpace ].Blight.Any );
	}

	[Trait( "Feature", "Defend" )]
	[Fact]
	public async Task Level1_DefendOnly1AndNotMorePerDahan() { // not more th
		Given_DahanAndTowns( 4, 4 );
		// 4 dahan should defend 4

		// When: Doing Invader phase (fear+ragage)

		await new DahanOnTheirGuard().ActAsync(1);

		await invaderCard.When_Ravaging();

		// Then: 0 dahan left
		ravageSpace.ScopeSpace.Dahan.CountAll.ShouldBe( 2 );

		//   And: 2 towns
		ravageSpace.Assert_HasInvaders( "2T@2" );
		gameState.Tokens[ ravageSpace ].Blight.Any.ShouldBe( true );

	}

	void Given_DahanAndTowns( int desiredDahan, int desiredTown ) {
		ravageSpace.ScopeSpace.Dahan.Init( desiredDahan );
		Assert.Equal(desiredDahan,ravageSpace.ScopeSpace.Dahan.CountAll);

		gameState.Tokens[ravageSpace].AdjustDefault( Human.Town, desiredTown );
	}

	readonly GameState gameState;
	readonly InvaderCard invaderCard;
	readonly SpaceSpec ravageSpace;
	readonly Spirit spirit;

}
