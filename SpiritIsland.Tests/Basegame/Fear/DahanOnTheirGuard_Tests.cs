namespace SpiritIsland.Tests.Basegame.Fear;

public class DahanOnTheirGuard_Tests {

	#region constructor

	public DahanOnTheirGuard_Tests() {
		spirit = new LightningsSwiftStrike();
		User = new VirtualUser( spirit );
		gameState = new GameState( spirit, Board.BuildBoardA() );
		gameState.DisableInvaderDeck();
		gameState.Initialize();
		gameState.Fear.Deck.Pop();
		gameState.Fear.AddCard( new DahanOnTheirGuard() );

		invaderCard = InvaderDeck.Level1Cards[0];
		ravageSpace = gameState.Island.Boards[0].Spaces.Where( invaderCard.Matches ).First();
	}

	#endregion

	[Trait( "Feature", "Defend" )]
	[Fact]
	public async Task NoFearCard_NormalRavage() {

		// Disable destroying presence
		gameState.ModifyBlightAddedEffect.ForGame.Add( x => { x.Cascade = false; x.DestroyPresence = false; } );

		Given_DahanAndTowns( 2, 2 );

		// When: Doing Invader phase (fear+ragage)
		await gameState.Fear.Apply();
		await InvaderEngine1.RavageCard( invaderCard, gameState );

		// Then: all dahan killed
		Assert.Equal( 0, gameState.DahanOn( ravageSpace ).Count );
		Assert.True( gameState.Tokens[ ravageSpace ].Blight.Any );
	}

	[Trait( "Feature", "Defend" )]
	[Fact]
	public void Level1_DefendOnly1AndNotMorePerDahan() { // not more th
		Given_DahanAndTowns( 4, 4 );
		// 4 dahan should defend 4

		//   And: 4 fear / player
		gameState.Fear.AddDirect( new FearArgs{count=4} );

		// When: Doing Invader phase (fear+ragage)
		async Task DoIt() {
			await gameState.Fear.Apply();
			await InvaderEngine1.RavageCard( invaderCard, gameState );
		}
		_ = DoIt();
		User.AcknowledgesFearCard( "Dahan on their Guard : 1 : In each land, Defend 1 per Dahan." );

		// Then: 0 dahan left
		gameState.DahanOn( ravageSpace ).Count.ShouldBe( 2 );

		//   And: 2 towns
		gameState.Assert_Invaders(ravageSpace, "2T@2" );
		gameState.Tokens[ ravageSpace ].Blight.Any.ShouldBe( true );

	}

	void Given_DahanAndTowns( int desiredDahan, int desiredTown ) {
		gameState.DahanOn( ravageSpace ).Init( desiredDahan );
		Assert.Equal(desiredDahan,gameState.DahanOn(ravageSpace).Count);

		gameState.Tokens[ravageSpace].AdjustDefault( Invader.Town, desiredTown );
	}

	readonly GameState gameState;
	readonly IInvaderCard invaderCard;
	readonly Space ravageSpace;
	readonly Spirit spirit;
	readonly VirtualUser User;

}