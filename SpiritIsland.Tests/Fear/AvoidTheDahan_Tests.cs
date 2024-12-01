using SpiritIsland.Log;

namespace SpiritIsland.Tests.Fear;

// For calculating pre-ravage invaders required to end with known post-ravage invaders/dahan use:
// Town(start) = Town(end) + Dahan(end)
// Dahan(start) = Town(start) + Dahan(end)

public class AvoidTheDahan_Tests {

	const string Level1Text = "Avoid the Dahan : 1 : Invaders do not Explore into lands with at least 2 Dahan.";
	const string Level2Text = "Avoid the Dahan : 2 : Invaders do not Build in lands where Dahan outnumber Town/City.";

	#region constructor

	void Init() {
		// On Board-A,
		// use A7 (Sands-2 Dahan)
		// or A4 (Sands-no dahan)
		var (user, spirit, task) = TestSpirit.StartGame( PowerCard.For(typeof(RiversBounty)), gs => {
			var fear = gs.Fear;
			InitMountainThenAllSands( gs );
			gs.NewLogEntry += (s) => log.Add(s.Msg());
		} );
		_user = user;
		_spirit = spirit;

		// Disable destroying presence
		GameState.Current.DisableBlightEffect();
		_waitForFearCard = GameState.Current.WatchForFearCard();

	}
	Task<FearCardRevealed> _waitForFearCard;

	#endregion

	[Trait( "Invaders", "Explore" )]
	[Fact]
	public async Task NullFearCard_NormalExplore() {

		Init();

		var spaceCtx = _spirit.TargetSpace( "A7" );
		spaceCtx.Space.Summary.ShouldBe( "2D@2" );

		ActivateFearCard(new NullFearCard());

		ClearBlightAndDoNothingForARound();
		await AcknowledgeFearCard( "Null Fear Card : 1 : x" );

		_user.WaitForNext();
		spaceCtx.Space.InvaderSummary().ShouldBe( "1E@1" );
	}

	[Trait( "Invaders", "Explore" )]
	[Fact]
	public async Task Level1_NoExplore() {

		// On Board-A,
		// use A7 (Sands-2 Dahan)
		// or A4 (Sands-no dahan)
		Spirit spirit = new TestSpirit( PowerCard.For(typeof(RiversBounty)) );
		GameState gs = new GameState( spirit, Board.BuildBoardA() ) {
			InvaderDeck = InvaderDeckBuilder.Default.Build() // Same order every time
		};
		InitMountainThenAllSands( gs );
		gs.NewLogEntry += ( s ) => log.Add( s.Msg() );
		gs.Initialize(); // Explore mountains
		_waitForFearCard = gs.WatchForFearCard();
		GameState.Current.DisableBlightEffect();

		var user = new VirtualTestUser( spirit );
		var a7 = gs.Island.Boards[0][7].ScopeSpace;
		a7.Summary.ShouldBe( "2D@2" );
		ActivateFearCard( new AvoidTheDahan() ); // Invaders do not explore into lands with at least 2 dahan

		Task task = InvaderPhase.ActAsync(gs);
		await AcknowledgeFearCard( Level1Text ); // explore Sands, Build Mountains
		await task.ShouldComplete();

		a7.InvaderSummary().ShouldBe( "" );
	}

	async Task AcknowledgeFearCard( string text) {
		FearCardRevealed fc = await _waitForFearCard;
		fc.Msg().ShouldBe(text);
	}

	[Trait( "Invaders", "Explore" )]
	[Fact]
	public async Task Level1_DahanKilledDuringRavage_ShouldExplore() {
		Init();

		var spaceCtx = _spirit.TargetSpace( "A7" );
		spaceCtx.Space.Summary.ShouldBe( "2D@2" );

		ClearBlightAndDoNothingForARound();
		_user.WaitForNext();
		spaceCtx.Space.Summary.ShouldBe( "2D@2,1E@1" );

		ClearBlightAndDoNothingForARound();
		_user.WaitForNext();
		spaceCtx.Space.Summary.ShouldBe( "2D@2,2E@1,1T@2" );

		// When: activating: 'Avoid the Dahan'
		ActivateFearCard( new AvoidTheDahan() );

		ClearBlightAndDoNothingForARound();
		await AcknowledgeFearCard(Level1Text);

		// Ravage should kill both dahan                   => 2 explorers and 1 town
		// Build should make a city                        => 2 explorers, 1 town, 1 city
		// Explore should add an explorer (dahan are gone) => 3 explorers, 1 town, 1 city

		_user.WaitForNext();
		spaceCtx.Space.InvaderSummary().ShouldBe( "1C@3,1T@2,3E@1" );

	}

	[Trait( "Invaders", "Build" )]
	[Fact]
	public async Task Level2_3DahanAtFear_1DahanAtBuild_DoBuild() {
		Init();


		// Fill all Invaders spaces with the A7 card
		// Round 1
		_user.Grows();
		_user.IsDoneBuyingCards();
		_user.WaitForNext();

		// Round 2
		_spirit.ClearAllBlight();
		_user.Grows();
		_user.IsDoneBuyingCards();
		_user.WaitForNext();

		// Round 3
		ElevateTerrorLevelTo(2);
		ActivateFearCard( new AvoidTheDahan() );
		// Given: Starting out Dahan(3) outnumber town/city(2)
		var spaceCtx = _spirit.TargetSpace( "A7" );
		spaceCtx.Space.Given_InitSummary( "3D@2,2T@2" );

		// When: activating fear
		_spirit.ClearAllBlight();
		_user.Grows();
		_user.IsDoneBuyingCards();
		//_user.AcknowledgesFearCard( Level2Text );
		await AcknowledgeFearCard(Level2Text);

		// Ravage: 2 towns kill 2 dahan leaving 1, 1 dahan kills 1 town leaving 1:  1B@1,1D@2,1T@2

		// Then: Dahan DO NOT outnumber the towns and cities any more! (1 & 1)
		//  But: we DO have some dahan

		// Build: Build city:	1B@1,1C@3,1D@2,1T@2
		// Explore: +1			1B@1,1C@3,1D@2,1E@1,1T@2
		_user.WaitForNext();
		spaceCtx.Space.Summary.ShouldBe( "1B,1C@3,1D@2,1E@1,1T@2" );
	}

	[Trait( "Invaders", "Build" )]
	[Fact]
	public async Task Level2_NoBuild() {

		var gs = new GameState(new RiverSurges(), Boards.D);
		var a5 = gs.Island.Boards[0][5].ScopeSpace;

		// Given: Dahan outnumber Towns+Cities
		const string originalTokens = "3C@1,5D@2,1T@1";
		a5.Given_InitSummary(originalTokens);

		//  When: Avoid the Dahan - Level 2 "Invaders do not build in lands where Dahan outnumber Town/City"
		await new AvoidTheDahan().ActAsync(2);

		//   And: Build on that land
		await a5.When_CardBuilds();

		//  Then: no build happened.
		a5.Summary.ShouldBe(originalTokens);
	}

	[Trait( "Invaders", "Build" )]
	[Fact]
	public async Task Level3_DahanCountedAtBuild_NotAtFear() {

		var gs = new GameState(new Shadows(), Boards.C);
		var c5 = gs.Island.Boards[0][5].ScopeSpace;

		// Given: space with Invaders (1 Town) AND 3 Dahan
		c5.Given_InitSummary("3D@2,1T@2");

		//   And: Avoid the Dahan - 3 activated (with 3 dahan present
		await new AvoidTheDahan().ActAsync(3);

		//   And: Dahan are removed (maybe via Ravage)
		c5.InitDefault(Human.Dahan,0);

		// When: Build
		await c5.When_CardBuilds();

		// Then: invaders Build.
		c5.Summary.ShouldBe("1C@3,1T@2");
	}


	#region private

	static void InitMountainThenAllSands( GameState gs ) {
		var sand = InvaderCard.Stage1( Terrain.Sands );
		gs.InitTestInvaderDeck(
			InvaderCard.Stage1( Terrain.Mountain ), // initial explorer in mountains
			sand, sand, sand, sand, sand
		);
	}

	static void ElevateTerrorLevelTo( int desiredFearLevel ) {
		while(GameState.Current.Fear.TerrorLevel < desiredFearLevel)
			GameState.Current.Fear.Deck.Pop();
	}

	static void ActivateFearCard(IFearCard fearCard) {
		fearCard.ActAsync(GameState.Current.Fear.TerrorLevel);


		//var fear = GameState.Current.Fear;
		//fear.Deck.Pop();				// discard card we are replacing
		//fear.PushOntoDeck(fearCard);    // push desired card onto the deck
		//fear.Add( fear.PoolMax );
	}


	void ClearBlightAndDoNothingForARound() {
		_spirit.ClearAllBlight();
		_user.GrowAndBuyNoCards();
	}

	VirtualTestUser _user;
	Spirit _spirit;
	readonly List<string> log = [];

	#endregion
}
