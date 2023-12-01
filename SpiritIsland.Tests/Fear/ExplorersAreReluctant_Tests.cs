namespace SpiritIsland.Tests.Fear;

public sealed class ExplorersAreReluctant_Tests {

	void Init() {
		var powerCard = PowerCard.For(typeof(CallToTend));

		(_user, _ctx) = TestSpirit.StartGame( powerCard );

		_log = GameState.Current.LogInvaderActions();
		_log.Clear(); // skip over initial Explorer setup
	}

	VirtualTestUser _user;
	SelfCtx _ctx;
	Queue<string> _log;

	void GrowAndBuyNoCards() {
		_ctx.ClearAllBlight();
		_user.GrowAndBuyNoCards();
	}

	[Trait( "Invaders", "Explore" )]
	[Trait( "Invaders", "Deck" )]
	[Fact]
	public async Task NormalInvaderPhases() {

		var spirit = new TestSpirit( PowerCard.For(typeof(CallToTend)) );

		GameState gs = new GameState( spirit, Board.BuildBoardA() ) {
			InvaderDeck = InvaderDeckBuilder.Default.Build() // Same order every time
		};
		gs.Initialize(); 

		// Disable destroying presence
		GameState.Current.DisableBlightEffect();
		_log = GameState.Current.LogInvaderActions();
		_log.Clear(); // clear out initial Explorer setup

		await InvaderPhase.ActAsync( gs );
		_log.Assert_Built( "A3", "A8" );
		_log.Assert_Explored( "A2", "A5" );

		await InvaderPhase.ActAsync( gs );
		_log.Assert_Ravaged( "A3", "A8" );
		_log.Assert_Built( "A2", "A5" );
		_log.Assert_Explored( "A4", "A7" );

		await InvaderPhase.ActAsync( gs );
		_log.Assert_Ravaged( "A2", "A5" );
		_log.Assert_Built( "A4", "A7" );
		_log.Assert_Explored( "A3", "A8" );

	}

	[Trait( "Invaders", "Explore" )]
	[Trait( "Invaders", "Deck" )]
	[Fact]
	public void Level1_SkipExploreInLowestNumberedLand() {

		Init();

		// Disable destroying presence
		GameState.Current.DisableBlightEffect();


		// 1: "During the next normal explore, skip the lowest-numbered land matching the invader card on each board.

		GrowAndBuyNoCards();

		_user.WaitForNext();

		// Ravage:-, Build:Jungle, Explore: Wetland
		_log.Assert_Built( "A3", "A8" );    // Jungle
		_log.Assert_Explored( "A2", "A5" ); // Water

		// Given: Explorers Are Reluctant
		_ctx.ActivateFearCard(new ExplorersAreReluctant());

		GrowAndBuyNoCards();
		_user.AcknowledgesFearCard("Explorers are Reluctant : 1 : During the next normal explore, skip the lowest-numbered land matching the invader card on each board.");
		_user.WaitForNext();

		// Ravage:Jungle, Build:Wetland, Explore: Sand
		_log.Assert_Ravaged( "A3", "A8" );
		_log.Assert_Built( "A2", "A5" );
		_log.Assert_Explored( "A7" ); // Skipped A4

		GrowAndBuyNoCards();
	}

	[Trait( "Invaders", "Explore" )]
	[Trait( "Invaders", "Deck" )]
	[Fact]
	public void Level2_DelayExplore1Round() {

		Init();

		// Disable destroying presence
		GameState.Current.DisableBlightEffect();

		// 2: Skip the next normal explore.  During the next invader phase, draw an adidtional explore card.

		// Card Advance #1 - Turn up first Explore Card
		// Card Advance #2 - Advance Explore Card to Build

		GrowAndBuyNoCards();

		_user.WaitForNext();
		_log.Assert_Built( "A3", "A8" );
		_log.Assert_Explored( "A2", "A5" );

		// Card Advance #3 - End of 1st round

		// Given: Explorers Are Reluctant
		_ctx.ActivateFearCard( new ExplorersAreReluctant() );
		//   And: Terror Level 2
		_ctx.ElevateTerrorLevelTo( 2 );

		GrowAndBuyNoCards();
		_user.AcknowledgesFearCard( "Explorers are Reluctant : 2 : Skip the next normal explore. During the next invader phase, draw an adidtional explore card." );
		System.Threading.Thread.Sleep(5);

		// Card Advance #4 - End of 2st round

		_user.WaitForNext();
		_log.Assert_Ravaged( "A3", "A8" );
		_log.Assert_Built( "A2", "A5" );

		GrowAndBuyNoCards();

		_user.WaitForNext();
		_log.Assert_Ravaged( "A2", "A5" );
		_log.Assert_Explored( "A4", "A7" ); // A4 & A7 happen together with next
		_log.Assert_Explored( "A3", "A8" ); // A4 & A7 happen together with next

	}

	[Trait( "Invaders", "Explore" )]
	[Trait( "Invaders", "Deck" )]
	[Fact]
	public void Level3_DelayExplore1Round() {

		Init();

		// Disable destroying presence
		GameState.Current.DisableBlightEffect();

		// 3: Skip the next normal explore, but still reveal a card. Perform the flag if relavant. Cards shift left as usual.

		// When Round 1 comes and goes
		GrowAndBuyNoCards(); // Begin Round 1
		_user.WaitForNext(); // End Round 1

		// Then:
		_log.Assert_Built( "A3", "A8" );
		_log.Assert_Explored( "A2", "A5" );

		// Given: Explorers Are Reluctant
		_ctx.ActivateFearCard(new ExplorersAreReluctant());
		_ctx.ElevateTerrorLevelTo( 3 );

		// When Round 2 comes and goes
		GrowAndBuyNoCards();
		_user.AcknowledgesFearCard("Explorers are Reluctant : 3 : Skip the next normal explore, but still reveal a card. Perform the flag if relavant. Cards shift left as usual.");
		_user.WaitForNext();

		// Then:
		_log.Assert_Ravaged( "A3", "A8" );
		_log.Assert_Built( "A2", "A5" );
		_log.Assert_Explored(); // Skipped A4 & A7

		// When: Round 3 comes and goes
		GrowAndBuyNoCards();
		_user.WaitForNext();

		// Then:
		_log.Assert_Ravaged( "A2", "A5" );
		_log.Assert_Built(/*"A4", "A7"*/); // we are currently filtering out spaces that have no explorers so they don't show up here.
		_log.Dequeue().ShouldStartWith( "No build due" );
		_log.Assert_Explored( "A3", "A8" ); // A4 & A7 happen together with next

	}

}