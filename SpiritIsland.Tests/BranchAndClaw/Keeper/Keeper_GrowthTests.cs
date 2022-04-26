namespace SpiritIsland.Tests.BranchAndClaw.Spirits.KeeperNS;

public class Keeper_GrowthTests : GrowthTests {

	static Spirit InitSpirit() {
		return new Keeper {
			CardDrawer = new PowerProgression(
				PowerCard.For<VeilTheNightsHunt>(),
				PowerCard.For<ReachingGrasp>()
			),
		};
	}

	readonly GameState gsbac;

	public Keeper_GrowthTests() : base( InitSpirit() ) {
		gsbac = new GameState( spirit, board );
		gameState = gsbac;
	}

	// a) reclaim, +1 energy
	// b) +1 power card
	// c) add presense range 3 containing wilds or presence, +1 energy
	// d) -3 energy, +1 power card, add presense to land without blight range 3

	[Fact]
	public void A_Reclaim_Energy_B_Powercard() {
		// a) reclaim, +1 energy
		// b) +1 power card
		Given_HalfOfPowercardsPlayed();

		When_StartingGrowth();
		User_Activates_A();
		User_Activates_B();

		Assert_AllCardsAvailableToPlay( 1 + 4 );
		Assert_HasEnergy( 1 + 2 );
		Assert_HasPowerProgressionCard( 0 );
	}

	[Fact]
	public void A_Reclaim_Energy_C_Presence_Energy() {
		// a) reclaim, +1 energy
		// c) add presense range 3 containing (wilds or presense), +1 energy

		Given_HasPresence( board[3] );
		Given_HalfOfPowercardsPlayed();
		Given_HasWilds( board[8] ); // 3 spaces away

		gameState.Phase = Phase.Growth;

		When_StartingGrowth();
		User_Activates_A();
		User_Activates_C();

		Assert_AllCardsAvailableToPlay();      // A
		Assert_HasEnergy( 2 + 2 );             // A & C
		Assert_BoardPresenceIs( "A3A3" );      // C
	}


	[Fact]
	public void A_Reclaim_Energy_D_Presence_PowerCard_LoseEnergy() {
		spirit.Energy = 10; // so we can -3 it
		// a) reclaim, +1 energy
		// d) -3 energy, +1 power card, add presense to land without blight range 3

		// Given: presence on board A  (default island is Board A)
		Given_HalfOfPowercardsPlayed();
		Given_HasPresence( board[3] );
		Given_BlightEverywhereExcept7();

		When_StartingGrowth();
		User_Activates_A();
		User_Activates_D();

		Assert_AllCardsAvailableToPlay( 4+1);     // A
		Assert_HasEnergy( 10 + 2-3+1 );                // A & D
		Assert_HasPowerProgressionCard(0); // D
		Assert_BoardPresenceIs( "A3A7" );        // D

	}

	[Fact]
	public void B_Powercard_C_Presence_Energy() {
		// b) +1 power card
		// c) add presense range 3 containing (wilds or presense), +1 energy

		// Given: presence at A3  (default island is Board A)
		Given_HasPresence( board[3] );
		// Given: 1 wilds, 3 away
		Given_HasWilds( board[8] );

		When_StartingGrowth();
		User_Activates_B();
		User_Activates_C();

		Assert_HasPowerProgressionCard( 0); // B
		Assert_HasEnergy( 1 + 2 );             // C
		Assert_BoardPresenceIs( "A3A3" );     // C
	}

	[Fact]
	public void B_Powercard_D_Presence_PowerCard_LoseEnergy() {
		// b) +1 power card
		// d) -3 energy, +1 power card, add presense to land without blight range 3

		// Given: presence on board A  (default island is Board A)
		Given_HasPresence( board[3] );
		Given_BlightEverywhereExcept7();
		spirit.Energy = 10; // so we can do this option

		gameState.Phase = Phase.Growth;
		When_StartingGrowth();
		User_Activates_B();
		User_Activates_D();

		Assert_HasPowerProgressionCard( 0); // B
		Assert_HasPowerProgressionCard( 1 ); // B
		Assert_HasCardAvailable( "Reaching Grasp" ); // D
		Assert_BoardPresenceIs( "A3A7" );     // D
		Assert_HasEnergy( 10 + 2 - 3 );
	}

	[Fact]
	public void C_Presence_Energy_D_Presence_PowerCard_LoseEnergy() {
		const int startingEnergy = 10;
		spirit.Energy = startingEnergy;
		// c) add presense range 3 containing (wilds or presense), +1 energy
		// d) -3 energy, +1 power card, add presense to land without blight range 3

		// Given: presence on board A  (default island is Board A)
		Given_HasPresence( board[3] );
		Given_HasWilds( board[8] );
		Given_BlightEverywhereExcept7();

		When_StartingGrowth();
		User_Activates_C();
		User_Activates_D();

		Assert_HasEnergy( startingEnergy + spirit.EnergyPerTurn - 2  );          // C & D
		Assert_HasPowerProgressionCard(0); // D

	}

	[Trait("Feature","Push")]
	[Fact]
	public void SacredSitesPushDahan() {
		// Given: space with 2 dahan
		var space = board[5];
		gameState.DahanOn(space).Init(2);
		//   and presence on that space
		spirit.Presence.PlaceOn( space, gameState );

		// When: we place a presence on that space
		_ = spirit.Presence.Place( spirit.Presence.Energy.RevealOptions.Single(), space, gameState );

		User.PushesTokensTo("D@2","A1,(A4),A6,A7,A8",2);
		User.PushesTokensTo("D@2","A1,A4,A6,(A7),A8");

		spirit.Presence.SacredSites.ShouldContain(space);
		gameState.Tokens[space].Dahan.Count.ShouldBe(0,"SS should push dahan from space");
	}


	[Theory]
	[InlineDataAttribute( 1, 2, "" )]
	[InlineDataAttribute( 2, 2, "1 sun" )]
	[InlineDataAttribute( 3, 4, "1 sun" )]
	[InlineDataAttribute( 4, 5, "1 sun" )]
	[InlineDataAttribute( 5, 5, "1 sun,1 plant" )]
	[InlineDataAttribute( 6, 7, "1 sun,1 plant" )]
	[InlineDataAttribute( 7, 8, "1 sun,1 plant" )]
	[InlineDataAttribute( 8, 9, "1 sun,1 plant" )]
	public Task EnergyTrack( int revealedSpaces, int expectedEnergyGrowth, string elements ) {
		var fix = new ConfigurableTestFixture { Spirit = new Keeper() };
		return fix.VerifyEnergyTrack( revealedSpaces, expectedEnergyGrowth, elements );
	}

	[Theory]
	[InlineDataAttribute( 1, 1 )]
	[InlineDataAttribute( 2, 2 )]
	[InlineDataAttribute( 3, 2 )]
	[InlineDataAttribute( 4, 3 )]
	[InlineDataAttribute( 5, 4 )]
	[InlineDataAttribute( 6, 5 )]
	public Task CardTrack( int revealedSpaces, int expectedCardPlayCount ) {
		var fix = new ConfigurableTestFixture { Spirit = new Keeper() };
		return fix.VerifyCardTrack(revealedSpaces,expectedCardPlayCount,"");
	}

	void AddBlight( Space space ) {
		gameState.Tokens[ space ].Blight.Add( 1 ).Wait(); // if cascading is not desired, try adjust
	}

	void Given_BlightEverywhereExcept7() {
		AddBlight( board[1] );
		AddBlight( board[2] );
		AddBlight( board[3] );
		AddBlight( board[4] );
		AddBlight( board[5] );
		AddBlight( board[6] );
		AddBlight( board[8] );
		gameState.Tokens[ board[7] ].Blight.Count.ShouldBe( 0 );
	}

	void Given_HasWilds( Space space ) {
		gameState.Tokens[space].Wilds.Init(1);
	}

	void User_Activates_A() {
		User.Growth_SelectsOption( "ReclaimAll / GainEnergy(1)" );
//			User.Growth_ReclaimsAll();
//			User.Growth_GainsEnergy();
	}

	void User_Activates_B() {
		User.Growth_SelectsOption( "DrawPowerCard" );
		// User.Growth_DrawsPowerCard(); - not needed due to AutoSelectSingle
	}

	void User_Activates_C() {
		User.Growth_SelectsOption( "GainEnergy(1) / PlacePresence(3,presence or wilds)" );
//		User.Growth_GainsEnergy();
		User.Growth_PlacesEnergyPresence( "A3;A8" );
	}

	void User_Activates_D() {
		User.Growth_SelectsOption( "GainEnergy(-3) / DrawPowerCard / PlacePresence(3,no blight)" );
//		User.Growth_GainsEnergy();
		User.Growth_DrawsPowerCard();
		User.PlacesCardPlayPresence( "A7" );
	}

}