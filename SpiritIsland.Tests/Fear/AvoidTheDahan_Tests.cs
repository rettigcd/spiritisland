using SpiritIsland.Log;

namespace SpiritIsland.Tests.Fear;

// For calculating pre-ravage invaders required to end with known post-ravage invaders/dahan use:
// Town(start) = Town(end) + Dahan(end)
// Dahan(start) = Town(start) + Dahan(end)

public class AvoidTheDahan_Tests {

	const string Level1Text = "Avoid the Dahan : 1 : Invaders do not Explore into lands with at least 2 Dahan.";
	const string Level2Text = "Avoid the Dahan : 2 : Invaders do not Build in lands where Dahan outnumber Town / City.";
	const string Level3Text = "Avoid the Dahan : 3 : Invaders do not Build in lands with Dahan.";

	#region constructor

	void Init() {
		// On Board-A,
		// use A7 (Sands-2 Dahan)
		// or A4 (Sands-no dahan)
		var (user, spirit) = TestSpirit.StartGame( PowerCard.For(typeof(RiversBounty)), gs => {
			var fear = gs.Fear;
			InitMountainThenAllSands( gs );
			gs.NewLogEntry += (s) => log.Add(s.Msg());
		} );
		_user = user;
		_spirit = spirit;

		// Disable destroying presence
		GameState.Current.DisableBlightEffect();

	}

	#endregion

	[Trait( "Invaders", "Explore" )]
	[Fact]
	public void NullFearCard_NormalExplore() {

		Init();

		var spaceCtx = _spirit.TargetSpace( "A7" );
		spaceCtx.Tokens.Summary.ShouldBe( "2D@2" );

		ActivateFearCard(new NullFearCard());

		ClearBlightAndDoNothingForARound();
		_user.AcknowledgesFearCard( "Null Fear Card : 1 : x" );

		_user.WaitForNext();
		spaceCtx.Tokens.InvaderSummary().ShouldBe( "1E@1" );
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
		GameState.Current.DisableBlightEffect();

		var user = new VirtualTestUser( spirit );
		var a7 = gs.Island.Boards[0][7].Tokens;
		a7.Summary.ShouldBe( "2D@2" );
		ActivateFearCard( new AvoidTheDahan() ); // Invaders do not explore into lands with at least 2 dahan

		Task task = InvaderPhase.ActAsync(gs);
		user.AcknowledgesFearCard( Level1Text ); // explore Sands, Build Mountains
		await task.ShouldComplete();

		a7.InvaderSummary().ShouldBe( "" );
	}

	[Trait( "Invaders", "Explore" )]
	[Fact]
	public void Level1_DahanKilledDuringRavage_ShouldExplore() {
		Init();


		var spaceCtx = _spirit.TargetSpace( "A7" );
		spaceCtx.Tokens.Summary.ShouldBe( "2D@2" );

		ClearBlightAndDoNothingForARound();
		_user.WaitForNext();
		spaceCtx.Tokens.Summary.ShouldBe( "2D@2,1E@1" );

		ClearBlightAndDoNothingForARound();
		_user.WaitForNext();
		spaceCtx.Tokens.Summary.ShouldBe( "2D@2,2E@1,1T@2" );

		// When: activating: 'Avoid the Dahan'
		ActivateFearCard( new AvoidTheDahan() );

		ClearBlightAndDoNothingForARound();
		_user.AcknowledgesFearCard( Level1Text );

		// Ravage should kill both dahan                   => 2 explorers and 1 town
		// Build should make a city                        => 2 explorers, 1 town, 1 city
		// Explore should add an explorer (dahan are gone) => 3 explorers, 1 town, 1 city

		_user.WaitForNext();
		spaceCtx.Tokens.InvaderSummary().ShouldBe( "1C@3,1T@2,3E@1" );

	}

	[Trait( "Invaders", "Build" )]
	[Fact]
	public void Level2_3DahanAtFear_1DahanAtBuild_DoBuild() {
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
		ActivateFearCard( new AvoidTheDahan() );
		ElevateTerrorLevelTo(2);
		// Given: Starting out Dahan(3) outnumber town/city(2)
		var spaceCtx = _spirit.TargetSpace( "A7" );
		spaceCtx.Tokens.Init( "3D@2,2T@2" );

		// When: activating fear
		_spirit.ClearAllBlight();
		_user.Grows();
		_user.IsDoneBuyingCards();
		_user.AcknowledgesFearCard( Level2Text );

		// Ravage: 2 towns kill 2 dahan leaving 1, 1 dahan kills 1 town leaving 1:  1B@1,1D@2,1T@2

		// Then: Dahan DO NOT outnumber the towns and cities any more! (1 & 1)
		//  But: we DO have some dahan

		// Build: Build city:	1B@1,1C@3,1D@2,1T@2
		// Explore: +1			1B@1,1C@3,1D@2,1E@1,1T@2
		_user.WaitForNext();
		spaceCtx.Tokens.Summary.ShouldBe( "1B,1C@3,1D@2,1E@1,1T@2" );
	}

	[Trait( "Invaders", "Build" )]
	[Fact]
	public void Level2_NoBuild() {
		Init();

		// "Invaders do not Build in lands where Dahan outnumber Town / City."

		// Fill all Invaders spaces with the A7 card
		// Round 1
		_user.Grows();
		_user.IsDoneBuyingCards();

		// Round 2
		_spirit.ClearAllBlight();
		_user.Grows();
		_user.IsDoneBuyingCards();
		_user.WaitForNext();

		// Round 3
		_spirit.ClearAllBlight();
		ActivateFearCard( new AvoidTheDahan() );
		ElevateTerrorLevelTo(2);
		// Given: Dahan(2) outnumber town/city(0)  + 3 explorer (to enable build)
		TargetSpaceCtx spaceCtx = _spirit.TargetSpace( "A7" );
		spaceCtx.Tokens.Init("2D@2,3E@1");

		// When: activating fear
		_user.Grows();
		_user.IsDoneBuyingCards();
		_user.AcknowledgesFearCard( Level2Text );

		// Then: no build
		// Ravage: 3 explorers kill 1 dahan, 1 dahan kills 2 explorer:  1B@1,1D@2,1E@1
		// Build: 1 dahan out numbers town/cities (0), no build:  1B@1,1D@2,1E@1
		// Explore: +1   1D@2,2E@1
		_user.WaitForNext();
		spaceCtx.Tokens.Summary.ShouldBe( "1B,1D@2,2E@1" );
	}

	[Trait( "Invaders", "Build" )]
	[Fact]
	public void Level3_3DahanAtFear_1DahanAtBuild_NoBuild() {
		Init();


		// Fill all Invaders spaces with the A7 card
		ClearBlightAndDoNothingForARound();
		ClearBlightAndDoNothingForARound();

		_user.WaitForNext();
		ActivateFearCard( new AvoidTheDahan() );
		ElevateTerrorLevelTo( 3 );

		// Given: Starting out Dahan(3) outnumber town/city(2)
		_user.WaitForNext();
		var spaceCtx = _spirit.TargetSpace( "A7" );
		spaceCtx.Tokens.Init( "3D@2,2T@2" );

		// When: activating fear
		ClearBlightAndDoNothingForARound();
		_user.AcknowledgesFearCard( Level3Text );

		// Ravage: 2 towns kill 2 dahan leaving 1, 1 dahan kills 1 town leaving 1:  1B@1,1D@2,1T@2

		// Then: Dahan do not out number cities/towns! (1 & 1)
		//  But: we do have SOME dahan

		// Build: no build      1B@1,1D@2,1T@2
		// Explore: +1			1B@1,1D@2,1E@1,1T@2
		_user.WaitForNext();
		spaceCtx.Tokens.Summary.ShouldBe( "1B,1D@2,1E@1,1T@2" );
	}

	[Trait( "Invaders", "Build" )]
	[Fact]
	public void Level3_1DahanAtFear_0DahanAtBuild_Build() {
		Init();

		// Fill all Invaders spaces with the A7 card
		ClearBlightAndDoNothingForARound();
		ClearBlightAndDoNothingForARound();

		_ = _user.NextDecision; // wait for engine to catch up
		ActivateFearCard( new AvoidTheDahan() );
		ElevateTerrorLevelTo(3);

		// Given: Starting out Dahan(3) outnumber town/city(2)
		var spaceCtx = _spirit.TargetSpace( "A7" );
		spaceCtx.Tokens.Init("1D@2,1T@2");

		// When: activating fear
		ClearBlightAndDoNothingForARound();
		_user.AcknowledgesFearCard( Level3Text );

		// Ravage: 1 towns kill 1 dahan leaving 0:  1B@1,1T@2

		// Then: dahan are gone

		// Build: build     1B@1,1C@3,1T@2
		// Explore: +1		1B@1,1C@3,1E@1,1T@2
		_ = _user.NextDecision; // wait for engine to catch up
		spaceCtx.Tokens.Summary.ShouldBe( "1B,1C@3,1E@1,1T@2" );
	}


	#region private

#pragma warning disable xUnit1013 // Public method should be marked as test
	static public void InitMountainThenAllSands( GameState gs ) {
		var sand = InvaderCard.Stage1( Terrain.Sands );
		gs.InitTestInvaderDeck(
			InvaderCard.Stage1( Terrain.Mountain ), // initial explorer in mountains
			sand, sand, sand, sand, sand
		);
	}
#pragma warning restore xUnit1013 // Public method should be marked as test

	static void ElevateTerrorLevelTo( int desiredFearLevel ) {
		while(GameState.Current.Fear.TerrorLevel < desiredFearLevel)
			GameState.Current.Fear.Deck.Pop();
	}

	static void ActivateFearCard(IFearCard fearCard) {
		var fear = GameState.Current.Fear;
		fear.Deck.Pop();				// discard card we are replacing
		fear.PushOntoDeck(fearCard);    // push desired card onto the deck
		fear.Add( fear.PoolMax );
	}


	void ClearBlightAndDoNothingForARound() {
		_spirit.ClearAllBlight();
		_user.GrowAndBuyNoCards();
	}

	VirtualTestUser _user;
	Spirit _spirit;
	readonly List<string> log = new();

	#endregion
}
