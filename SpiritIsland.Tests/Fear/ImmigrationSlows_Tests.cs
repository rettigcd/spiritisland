namespace SpiritIsland.Tests.Fear; 

public sealed class ImmigrationSlows_Tests {

	const string FearAck1 = "Immigration Slows : 1 : During the next normal build, skip the lowest-numbered land matching the Invader card on each board.";
	const string FearAck2 = "Immigration Slows : 2 : Skip the next normal Build. The Build card remains in place instead of shifting left.";
	const string FearAck3 = "Immigration Slows : 3 : Skip the next normal Build. The Build card shifts left as usual.";
	readonly IFearCard _fearCard = new ImmigrationSlows();

	[Trait( "Invaders", "Build" )]
	[Trait( "Invaders", "Deck" )]
	[Fact]
	public async Task Level1_SkipBuildInLowestNumberedLand() {
		var powerCard = PowerCard.For(typeof(CallToTend));
		var (user, spirit) = TestSpirit.StartGame(powerCard);
		_user = user; 
		_spirit = spirit;
		var gs = GameState.Current;
		_log = gs.LogInvaderActions();
		var fearCard = gs.WatchForFearCard();
		_log.Clear(); // skip over initial Explorer setup


		// 1: During the next normal build, skip the lowest-numbered land matching the invader card on each board.

		GrowAndBuyNoCards();
		_user.WaitForNext(); // start of round 2

		_log.Assert_Built( "A3", "A8" );
		_log.Assert_Explored( "A2", "A5" );

		// Given: Explorers Are Reluctant
		_spirit.ActivateFearCard( _fearCard );
		GrowAndBuyNoCards();
		(await fearCard).Msg().ShouldBe( FearAck1 ); // _user.AcknowledgesFearCard( FearAck1 );
		_user.WaitForNext(); // start of round 3

		_log.Assert_Ravaged( "A3", "A8" );
		_log.Assert_Built( "A2: build stopped", "A5" ); // Skipped A2
		_log.Assert_Explored( "A4", "A7" ); 

		GrowAndBuyNoCards();
		_user.WaitForNext(); // start of round 4

		_log.Assert_Ravaged( "A2", "A5" );
		_log.Assert_Built( "A4", "A7" );
		_log.Assert_Explored( "A3", "A8" );

	}

	[Trait( "Invaders", "Build" )]
	[Trait( "Invaders", "Deck" )]
	[Fact]
	public async Task Level2_DelayBuild1Round() {
		// 2: Skip the next normal build. The build card remains in place instead of shifting left.
		// (_user, _ctx) = TestSpirit.StartGame(PowerCard.For<CallToTend>());

		var spirit = new TestSpirit( PowerCard.For(typeof(CallToTend)) );
		var gs = new GameState( spirit, Board.BuildBoardA() ) {
			InvaderDeck = InvaderDeckBuilder.Default.Build() // Same order every time
		};
		var fearCard = gs.WatchForFearCard();
		gs.Initialize();   // Card #1 => Explore
		GameState.Current.DisableBlightEffect();
		_log = GameState.Current.LogInvaderActions();
		_log.Clear(); // skip over initial Explorer setup

		// Round 1
		await InvaderPhase.ActAsync(gs); // Card #2 - Advance to: Build then Explore
		_log.Assert_Built( "A3", "A8" );
		_log.Assert_Explored( "A2", "A5" );
		_log.Clear();

		// Given: Explorers Are Reluctant & Terror Level 2
		ActivateFearCard( gs, _fearCard );
		ElevateTerrorLevelTo( 2 );

		// When #1: Do Invader phase
		Task t = InvaderPhase.ActAsync(gs);
		(await fearCard).Msg().ShouldBe( FearAck2 );
		await t.ShouldComplete();

		// Then #1: there was a Ravage & Explore but NO build
		_log.Assert_Ravaged( "A3", "A8" );
		_log.Assert_Explored("A4","A7");
		_log.Clear();

		// When #2: Do Invader phase again
		await InvaderPhase.ActAsync(gs);

		// Then #2: we got 2 builds and an explore but no Ravage
		_log.Assert_Built( "A2", "A5" ); // double up Builds
		_log.Assert_Built( "A4", "A7" ); // double up Builds
		_log.Assert_Explored( "A3", "A8" );

	}

	static void ActivateFearCard( GameState gs, IFearCard fearCard ) {
		var fear = gs.Fear;
		fear.Deck.Pop();
		fear.PushOntoDeck(fearCard);
		fear.Add( fear.PoolMax );
	}

	static void ElevateTerrorLevelTo( int desiredFearLevel ) {
		while(GameState.Current.Fear.TerrorLevel < desiredFearLevel)
			GameState.Current.Fear.Deck.Pop();
	}


	[Trait( "Invaders", "Build" )]
	[Trait( "Invaders", "Deck" )]
	[Fact]
	public async void Level3_DelayExplore1Round() {
		var powerCard = PowerCard.For(typeof(CallToTend));
		var (user, spirit) = TestSpirit.StartGame(powerCard);
		_user = user;
		_spirit = spirit;
		var gs = GameState.Current;
		var fearCard = gs.WatchForFearCard();
		_log = gs.LogInvaderActions();
		_log.Clear(); // skip over initial Explorer setup

		// 3: Skip the next normal explore, but still reveal a card. Perform the flag if relavant. Cards shift left as usual.

		GrowAndBuyNoCards();

		_user.WaitForNext();
		_log.Assert_Built( "A3", "A8" );
		_log.Assert_Explored( "A2", "A5" );

		// Given: Explorers Are Reluctant
		_spirit.ActivateFearCard( _fearCard );
		_spirit.ElevateTerrorLevelTo( 3 );

		GrowAndBuyNoCards();
		(await fearCard).Msg().ShouldBe( FearAck3 );

		_user.WaitForNext();
		_log.Assert_Ravaged( "A3", "A8" );
		_log.Assert_Explored("A4", "A7");

		GrowAndBuyNoCards();

		_user.WaitForNext();
		_log.Assert_Ravaged( "A2", "A5" );
		_log.Assert_Built("A4", "A7"); // normal build
		_log.Assert_Explored( "A3", "A8" ); // A4 & A7 happen together with next

	}

	VirtualTestUser _user;
	Spirit _spirit;
	Queue<string> _log;

	void GrowAndBuyNoCards() {
		_spirit.ClearAllBlight();
		_user.GrowAndBuyNoCards();
	}

}

