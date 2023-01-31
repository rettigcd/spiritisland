using SpiritIsland.Log;

namespace SpiritIsland.Tests.Spirits.OceanNS;

public class OceanTerrain_Tests {

	#region private fields

	readonly Ocean oceanSpirit = new Ocean();
	readonly Board boardA = Board.BuildBoardA();
	readonly Spirit primarySpirit = new Thunderspeaker();
	readonly GameState gameState;
	DecisionContext NextDecision => primarySpirit.NextDecision();

	#endregion

	#region constructor

	public OceanTerrain_Tests() {
		gameState = Given_TwoSpiritGame();
	}

	#endregion

	[Trait("SpecialRule","OceanInPlay")]
	[Fact]
	public async Task CannotTargetOcean() {
		// Given: 2-spirit-game with Thundersepearker on A and Ocean on B

		//   And: Thundersepearker on A2 only
		Given_PrimaryPresenceOnA2Only();

		// When: Thundersepearker Activates a card that targets ANY-terrain Range-1 (Call To Guard - Range 1, Any land)
		await using UnitOfWork action = gameState.StartAction( ActionCategory.Spirit_Power );
		SelfCtx ctx = primarySpirit.BindMyPowers( gameState, action );
		_ = PowerCard.For<CallToGuard>().ActivateAsync( ctx );

		// Then: Targetting does not inculde Ocean
		NextDecision.HasOptions( "A1,A2,A3,A4" );
	}

	[Trait( "SpecialRule", "OceanInPlay" )]
	[Fact]
	public async Task WithOcean_CanTargetOceanAsWetland() {
		// Given: 2-spirit-game with Thundersepearker on A and Ocean on B

		//   And: Thundersepearker on A2 only
		Given_PrimaryPresenceOnA2Only();

		//   And: Ocean is on Thundersepearker's board
		Given_OceanOnPrimaryBoard();

		// When: Thundersepearker Activates a card that targets WETLANDS (Talons ofLightning - Range 1, M/W)
		await using UnitOfWork action = gameState.StartAction( ActionCategory.Spirit_Power );
		SelfCtx ctx = primarySpirit.BindMyPowers( gameState, action );
		_ = PowerCard.For<TalonsOfLightning>().ActivateAsync( ctx );

		// Then: Targetting options INCLUDES Ocean
		NextDecision.HasOptions( "A0,A1,A2" );

	}

	[Trait( "SpecialRule", "OceanInPlay")]
	[Trait( "SpecialRule", "Drowning" )]
	[Theory]
	[InlineData(false),InlineData(true)]
	public async Task PushDahanIntoOcean(bool withOcean) {
		// track starting energy
		int oceanStartingEnergy = oceanSpirit.Energy;

		// Given: 2-spirit-game with Thundersepearker on A and Ocean on B

		//   And: Thundersepearker on A2 only
		Given_PrimaryPresenceOnA2Only();

		if( withOcean )
			Given_OceanOnPrimaryBoard();

		// When: Thundersepearker Activates a card that Pushes Dahan
		// Call To Tend: Range 1, Dahan, Push up to 3 Dahan
		await using UnitOfWork action = gameState.StartAction( ActionCategory.Spirit_Power );
		SelfCtx ctx = primarySpirit.BindMyPowers( gameState, action );
		_ = PowerCard.For<CallToTend>().ActivateAsync( ctx );
		//  And: Targets A2 (that has a dahan on it)
		Choose( "A2" );
		//  And: Push a Dahan
		Choose( "D@2" );

		// Then: push options should not include ocean
		NextDecision.HasOptions( withOcean ? "A0,A1,A3,A4" : "A1,A3,A4" );

		if( withOcean) {
			var log = new List<string>();
			gameState.NewLogEntry += (e) => { if( e is Debug ) log.Add( e.Msg() ); };

			// Push into Ocean and let Thunderspeaker ride along
			Choose("A0");

			// bring Thunderspeaker along
			NextDecision.HasPrompt( "Move presence with Dahan?" )
				.HasOptions( "SpiritIsland.SpiritPresenceToken on A2,Done" )
				.Choose( "SpiritIsland.SpiritPresenceToken on A2" );
			
			// Then: This should destroy the dahan
			var oceanSpace = gameState.Tokens[boardA[0]];
			oceanSpace.Summary.ShouldBe("");
			log.Single().ShouldBe("Drowning 1D@2 on A0");

			//  And: and leave thunderspeaker in the ocean.
			primarySpirit.Presence.IsOn( oceanSpace ).ShouldBeTrue();

			// And should NOT adjust energy
			oceanSpirit.Energy.ShouldBe(oceanStartingEnergy);
		}
	}

	[Trait( "SpecialRule", "OceanInPlay" )]
	[Trait( "SpecialRule", "Drowning" )]
	[Theory]
	[InlineData( false,"T@2", 0 )] // lots of invaders you can't push into the ocean
	[InlineData( true, "E@1", 1 )] // push 2 explorers => 2 health => 1 energy
	[InlineData( true, "T@2", 2 )] // push 2 town => 4 health => 2 energy
	public async Task PushInvadersIntoOcean( bool withOcean, string pushToken, int expectedEnergyGain ) {
		// track starting energy
		int oceanStartingEnergy = oceanSpirit.Energy;
		var debugLog = new List<string>();
		gameState.NewLogEntry += ( e ) => { if(e is Debug) debugLog.Add( e.Msg() ); };

		// Given: 2-spirit-game with Thundersepearker on A and Ocean on B

		if(withOcean)
			Given_OceanOnPrimaryBoard();

		//   And: 2 towns and 2 explorers on space
		SpaceState a2 = gameState.Tokens[boardA[2]];
		a2.InitDefault( Invader.Town, 2 );
		a2.InitDefault( Invader.Explorer, 2 );

		//   And: Thundersepearker on A2 only
		Given_PrimaryPresenceOnA2Only();

		// When: Thundersepearker Activates a card that Pushes Explorers/Towns
		// Land of Haunts And Embers: Range 2, Any, Push up to 2 Explorers/Towns
		await using UnitOfWork action = gameState.StartAction( ActionCategory.Spirit_Power );
		SelfCtx ctx = primarySpirit.BindMyPowers( gameState, action );
		_ = PowerCard.For<LandOfHauntsAndEmbers>().ActivateAsync( ctx );
		//  And: Targets A2
		Choose( a2.Space.Text );

		for(int i=0; i < 2; ++i) {
			// When: Push 1st invadera Town
			Choose( pushToken );
			// Then: ocean is/is-not an option
			NextDecision
				.HasOptions( withOcean ? "A0,A1,A3,A4" : "A1,A3,A4" )
				.Choose(withOcean ? "A0" : "A1");
		}

		// Then: ocean should have drown energy
		oceanSpirit.Energy.ShouldBe( oceanStartingEnergy + expectedEnergyGain );
		debugLog
			.Count( x => x == $"Ocean gained 1 energy from cashing in 2 health of drowned invaders." )
			.ShouldBe( expectedEnergyGain );
	}

	[Trait( "Blight", "Cascade" )]
	[Trait( "SpecialRule", "OceanInPlay" )]
	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public void CascadingBlight(bool withOcean ) {
		// with blight in A2 already
		// Ravage in a2 (already has city)

		// Given: 2-spirit-game with Thundersepearker on A and Ocean on B

		//   And: With/Without Ocean on board
		if(withOcean)
			Given_OceanOnPrimaryBoard();

		//   And: blight on a2
		SpaceState a2 = gameState.Tokens[boardA[2]];
		a2.Blight.Init(1);
		//   But: no dahan (don't want to trigger Thunderspeakers presence destruction
		a2.Dahan.Init(0);

		//  When: invaders ravage and cause blight
		_ = boardA[2].DoARavage(gameState);

		// Then: we can/can't cascade into ocean
		NextDecision
			.HasOptions( withOcean ? "A0,A1,A3,A4" : "A1,A3,A4" )
			.Choose( withOcean ? "A0" : "A1" );
	}

	[Trait( "SpecialRule", "OceanInPlay" )]
	[Fact]
	public void OtherSpirits_CannotGrowInOcean() {
		// Given: With/Without Ocean on board
		Given_OceanOnPrimaryBoard();

		Given_PrimaryPresenceOnA2Only();

		// When: placing precense
		_ = primarySpirit.DoGrowth( gameState );
		NextDecision.Choose("PlacePresence(1)");

		//  And: take from card play
		NextDecision.Choose( "2 cardplay");

		// Then: ocean is not in option list
		NextDecision.HasOptions( "A1,A2,A3,A4" ).Choose( "A1" ); // close out action thread
	}

	[Trait( "SpecialRule", "OceanInPlay" )]
	[Fact]
	public void PlacePresenceInOcean_DuringPower() {
		// Given: With/Without Ocean on board
		Given_OceanOnPrimaryBoard();

		Given_PrimaryPresenceOnA2Only(); // reduces # of Target options

		//  And: spirit can use blazing renewal
		primarySpirit.Energy = 5;
		primarySpirit.Presence.Destroyed = 1;
		primarySpirit.AddActionFactory(PowerCard.For<BlazingRenewal>());
		gameState.Phase = Phase.Fast;

		// When: using a Power that places presence
		var task = primarySpirit.ResolveActions(gameState);
		IsActive(task);
		Choose( "Blazing Renewal $5 (Fast)" );
		IsActive( task );
		Choose( "Thunderspeaker" );

		// Then: ocean (A0) IS a destination to place presence
		Choose( "A0" );

	}

	[Trait("SpecialRule","OceanInPlay")]
	[Theory]
	[InlineData(true),InlineData(false)]
	public void TargetOutOfOceanAsWetland( bool withOcean ) {

		// Given: with/without ocean
		if( withOcean )
			Given_OceanOnPrimaryBoard();

		// Presence in Ocean
		Given_PrimaryPresenceOnlyOn( boardA[0]);

		// When: trying to use a card that targets out of a wetland
		// Cleansing Floods (range 1 from wetland)
		primarySpirit.Energy = 5;
		primarySpirit.AddActionFactory(PowerCard.For<CleansingFloods>());
		gameState.Phase = Phase.Slow;
		var actionTask = primarySpirit.ResolveActions( gameState );
		Choose( "Cleansing Floods $5 (Slow)" );

		if( withOcean) {
			// Then: can target out of wetland
			actionTask.IsCompleted.ShouldBeFalse();
			NextDecision.HasOptions("A0,A1,A2,A3");
		} else
			// Then: cannot target anything, done
			actionTask.IsCompleted.ShouldBeTrue();

	}

	[Trait("SpecialRule","OceanInPlay")]
	[Trait("SpecialRule","Drowning")]
	[Trait("SpecialRule","AllyOfTheDahan")]
	[Theory]
	[InlineData(true),InlineData (false)]
	public void TidalBoon_EnergyAndDahan(bool savedByOcean) {
		// Given thunderspeaker / ocean game
		var log = new List<string>();
		gameState.NewLogEntry += (x) => log.Add(x.Msg());

		// Given: Primary on A2
		Given_PrimaryPresenceOnA2Only();

		// Given: Dahan and Town on A2
		SpaceState a2 = gameState.Tokens[boardA[2]];
		a2.InitDefault( TokenType.Dahan, 2 );
		a2.InitDefault( Invader.Town, 1 );

		// Given: ocean in either A0 (saving dahan) or A1 (not saving)
		Space oceanSpace = savedByOcean ? boardA[0] : boardA[1];
		oceanSpirit.Presence.Adjust( gameState.Tokens[oceanSpace], 1 );

		// When: Tidal Boon is played (by Ocean)
		gameState.Phase = Phase.Slow;
		oceanSpirit.AddActionFactory( PowerCard.For<TidalBoon>() );
		Task task = oceanSpirit.ResolveActions( gameState );
		oceanSpirit.NextDecision().ChooseFirstThatStartsWith( TidalBoon.Name );

		//  And: Primary selects A2 space
		IsActive( task ); Choose( "A2" );

		//  And: Pushes town into ocean
		IsActive( task ); Choose( "T@2" );
		IsActive( task ); Choose( "A0" );

		// Then: Ocean gets 1 energy (2 health / 2 players = 1 energy)
		oceanSpirit.Energy.ShouldBe( 1 );

		// When: Pushes 1st Dahan into Ocean
		IsActive( task ); Choose( "D@2" );
		IsActive( task ); Choose( "A0" );
		if(savedByOcean) {
			// Ocean should decide if it is going to save them now
			oceanSpirit.NextDecision()
				.HasOptions( "A1,A2,A3,B1,B2,B3,Done" )
				.Choose( "A1" );
		}
		// Thunderspeaker goes along
		Choose( "SpiritIsland.SpiritPresenceToken on A2" );

		// When: Pushes 2nd dahan into Ocean
		IsActive( task ); Choose( "D@2" );
		IsActive( task ); Choose( "A0" );

		if(savedByOcean) {
			// Ocean should decide if it is going to save them now
			oceanSpirit.NextDecision()
				.HasOptions( "A1,A2,A3,B1,B2,B3,Done" )
				.Choose("A1");

			// End of Action - Thunder speaker exits ocean
			Choose( "SpiritIsland.SpiritPresenceToken on A0" );
		}
	}

	#region private helper methods

	GameState Given_TwoSpiritGame() {
		GameState gameState = new GameState( 
			new Spirit[]{ primarySpirit, oceanSpirit },
			new Board[] { boardA, Board.BuildBoardB() } 
		);
		gameState.Initialize();
		return gameState;
	}

	void Given_OceanOnPrimaryBoard() => oceanSpirit.Presence.Adjust( gameState.Tokens[boardA[0]], 1 ); // put ocean presence on primary's board, but not in the ocean

	void Given_PrimaryPresenceOnA2Only() => Given_PrimaryPresenceOnlyOn( boardA[2] );

	void Given_PrimaryPresenceOnlyOn( Space space ) {
		foreach(Space s in primarySpirit.Presence.Spaces( gameState ).ToArray())
			primarySpirit.Presence.Adjust( gameState.Tokens[s], -1 );

		// Add to
		primarySpirit.Presence.Adjust( gameState.Tokens[space], 1 );
		primarySpirit.Presence.Spaces( gameState ).Select( s => s.Text ).Join( "," ).ShouldBe( space.Text );
	}

	
	void Choose( string text ) => NextDecision.Choose( text );

	static void IsActive( Task task ) => task.IsCompleted.ShouldBeFalse();

	#endregion

}
