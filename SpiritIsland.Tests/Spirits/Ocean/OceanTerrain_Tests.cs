using SpiritIsland.Log;

namespace SpiritIsland.Tests.Spirits.OceanNS;

[Collection("BaseGame Spirits")]
public class OceanTerrain_Tests {

	#region private fields

	readonly Ocean _oceanSpirit;
	readonly Board _boardA;
	readonly Thunderspeaker _primarySpirit;
	readonly GameState _gameState;

	DecisionContext NextDecision => _primarySpirit.NextDecision();

	#endregion

	#region constructor

	public OceanTerrain_Tests() {
		_gameState = new GameConfiguration()
			.ConfigSpirits(Thunderspeaker.Name,Ocean.Name)
			.ConfigBoards("A","B")
			.BuildGame();
		_primarySpirit = (Thunderspeaker)_gameState.Spirits[0];
		_oceanSpirit = (Ocean)_gameState.Spirits[1];
		_boardA = _gameState.Island.Boards[0];
	}

	#endregion

	[Trait("SpecialRule","OceanInPlay")]
	[Fact]
	public async Task CannotTargetOcean() {
		// Given: 2-spirit-game with Thundersepearker on A and Ocean on B

		//   And: Thundersepearker on A2 only
		Given_PrimaryPresenceOnA2Only();

		// When: Thundersepearker Activates a card that targets ANY-terrain Range-1 (Call To Guard - Range 1, Any land)
		await _primarySpirit.When_ResolvingCard<CallToGuard>( (user) => {

			// Then: Targetting does not inculde Ocean
			user.NextDecision.HasOptions( "A1,A2,A3,A4" ).Choose("A1");
			// cleanup
			user.NextDecision.Choose("Done");
		} );

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
		await _primarySpirit.When_ResolvingCard<TalonsOfLightning>( (user) => {
			// Then: Targetting options INCLUDES Ocean
			user.NextDecision.HasOptions( "A0,A1,A2" ).Choose("A0");
		} );

	}

	[Trait( "SpecialRule", "OceanInPlay")]
	[Trait( "SpecialRule", "Drowning" )]
	[Trait( "SpecialRule", "Ally of the Dahan")]
	[Theory]
	[InlineData(false),InlineData(true)]
	public async Task PushDahanIntoOcean(bool withOcean) {
		// track starting energy
		int oceanStartingEnergy = _oceanSpirit.Energy;

		// Given: 2-spirit-game with Thundersepearker on A and Ocean on B

		//   And: Thundersepearker on A2 only
		Given_PrimaryPresenceOnA2Only();

		if( withOcean )
			Given_OceanOnPrimaryBoard();

		var log = new List<string>();

		// When: Thundersepearker Activates a card that Pushes Dahan
		// Call To Tend: Range 1, Dahan, Push up to 3 Dahan
		await using ActionScope action = await ActionScope.StartSpiritAction(ActionCategory.Spirit_Power,_primarySpirit); // required to signal it is a spirit power
		await PowerCard.ForDecorated(CallToTend.ActAsync).ActivateAsync( _primarySpirit ).AwaitUser( user => { 

			user.NextDecision.Choose("A2");	//  And: Targets A2 (that has a dahan on it)

			if(withOcean)
				user.NextDecision.ChooseFrom("D@2").ChooseTo("A0","A0,A1,A3,A4");
			else
				user.NextDecision.ChooseFrom("D@2").ChooseTo("A1","A1,A3,A4");

			_gameState.NewLogEntry += (e) => { if( e is Debug ) log.Add( e.Msg() ); };

			// bring Thunderspeaker along
 			user.NextDecision.HasPrompt( "Move presence with Dahan?" ).HasOptions( "Ts,Done" ).Choose( "Ts" );

		} ).ShouldComplete();


		if( withOcean ) {

			// Then: This should destroy the dahan
			var oceanSpace = _gameState.Tokens[_boardA[0]];
			oceanSpace.Summary.ShouldBe("1OHG,1Ts");
			log.Single().ShouldBe("Drowning 1D@2 on A0");

			//  And: and leave thunderspeaker in the ocean.
			oceanSpace.Has(_primarySpirit.Presence ).ShouldBeTrue();

			// And should NOT adjust energy
			_oceanSpirit.Energy.ShouldBe(oceanStartingEnergy);
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
		int oceanStartingEnergy = _oceanSpirit.Energy;
		var debugLog = new List<string>();
		_gameState.NewLogEntry += ( e ) => { if(e is Debug) debugLog.Add( e.Msg() ); };

		// Given: 2-spirit-game with Thundersepearker on A and Ocean on B

		if(withOcean)
			Given_OceanOnPrimaryBoard();

		//   And: 2 towns and 2 explorers on space
		Space a2 = _gameState.Tokens[_boardA[2]];
		a2.InitDefault( Human.Town, 2 );
		a2.InitDefault( Human.Explorer, 2 );

		//   And: Thundersepearker on A2 only
		Given_PrimaryPresenceOnA2Only();

		// When: Thundersepearker Activates a card that Pushes Explorers/Towns
		// Land of Haunts And Embers: Range 2, Any, Push up to 2 Explorers/Towns
		await using ActionScope action = await ActionScope.StartSpiritAction(ActionCategory.Spirit_Power,_primarySpirit);
		await PowerCard.ForDecorated(LandOfHauntsAndEmbers.ActAsync).ActivateAsync( _primarySpirit ).AwaitUser( user => {
			//  And: Targets A2
			user.NextDecision.Choose( a2.Label );

			for(int i=0; i < 2; ++i) {
				// When: Push 1st invadere - Town
				user.NextDecision.ChooseFrom( pushToken )
				// Then: ocean is/is-not an option
					.ChooseTo( 
						withOcean ? "A0" : "A1",
						withOcean ? "A0,A1,A3,A4" : "A1,A3,A4"
					);
			}

		} ).ShouldComplete();

		// Then: ocean should have drown energy
		_oceanSpirit.Energy.ShouldBe( oceanStartingEnergy + expectedEnergyGain );
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
		Space a2 = _gameState.Tokens[_boardA[2]];
		a2.Blight.Init(1);
		//   But: no dahan (don't want to trigger Thunderspeakers presence destruction
		a2.Dahan.Init(0);

		//  When: invaders ravage and cause blight
		_ = _boardA[2].ScopeSpace.Ravage();

		// Then: we can/can't cascade into ocean
		if(withOcean)
			NextDecision
				.HasOptions( "Blight on A2 => A0,Blight on A2 => A1,Blight on A2 => A3,Blight on A2 => A4" )
				.Choose("Blight on A2 => A4");
		else
			NextDecision
				.HasOptions( "Blight on A2 => A1,Blight on A2 => A3,Blight on A2 => A4")
				.Choose("Blight on A2 => A4");
	}

	[Trait( "SpecialRule", "OceanInPlay" )]
	[Fact]
	public void OtherSpirits_CannotGrowInOcean() {
		// Given: With/Without Ocean on board
		Given_OceanOnPrimaryBoard();

		Given_PrimaryPresenceOnA2Only();

		// When: placing precense
		_ = _primarySpirit.DoGrowth( _gameState );
		NextDecision.Choose("Place Presence(1)");

		NextDecision.ChooseFrom("2 cardplay").HasToOptions("A1,A2,A3,A4").ChooseTo("A1");

		////  And: take from card play
		//NextDecision.Choose( "2 cardplay");
		//// Then: ocean is not in option list
		//NextDecision.HasOptions( "A1,A2,A3,A4" ).Choose( "A1" ); // close out action thread
	}

	[Trait( "SpecialRule", "OceanInPlay" )]
	[Fact]
	public void PlacePresenceInOcean_DuringPower() {
		// Given: With/Without Ocean on board
		Given_OceanOnPrimaryBoard();

		Given_PrimaryPresenceOnA2Only(); // reduces # of Target options

		//  And: spirit can use blazing renewal
		_primarySpirit.Energy = 5;
		_primarySpirit.Presence.Destroyed.Count = 1;
		_primarySpirit.AddActionFactory(PowerCard.ForDecorated(BlazingRenewal.ActAsync));
		_gameState.Phase = Phase.Fast;

		// When: using a Power that places presence
		var task = _primarySpirit.SelectAndResolveActions(_gameState);
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
	public async Task TargetOutOfOceanAsWetland( bool withOcean ) {

		// Given: with/without ocean
		if( withOcean )
			Given_OceanOnPrimaryBoard();

		// Presence in Ocean
		Given_PrimaryPresenceOnlyOn( _boardA[0]);

		// When: trying to use a card that targets out of a wetland
		// Cleansing Floods (range 1 from wetland)
		_primarySpirit.Energy = 5;
		_primarySpirit.AddActionFactory(PowerCard.ForDecorated(CleansingFloods.ActAsync));
		_gameState.Phase = Phase.Slow;
		await _primarySpirit.SelectAndResolveActions( _gameState ).AwaitUser((user)=> {
			user.Choose( "Cleansing Floods $5 (Slow)" );
			if(withOcean) {
				// Then: can target out of wetland
				user.NextDecision.HasOptions( "A0,A1,A2,A3" ).Choose("A1");
			}
		} ).ShouldComplete("Cleansing Flood");

	}

	[Trait("SpecialRule","OceanInPlay")]
	[Trait("SpecialRule","Drowning")]
	[Trait("SpecialRule","AllyOfTheDahan")]
	[Theory]
	[InlineData(true),InlineData (false)]
	public async Task TidalBoon_EnergyAndDahan(bool savedByOcean) {

		// Given thunderspeaker / ocean game

		// Given: Primary on A2
		Given_PrimaryPresenceOnA2Only();

		// Given: Dahan and Town on A2
		Space a2 = _gameState.Tokens[_boardA[2]];
		a2.InitDefault( Human.Dahan, 2 );
		a2.InitDefault( Human.Town, 1 );

		// Given: ocean in either A0 (saving dahan) or A1 (not saving)
		SpaceSpec oceanSpace = savedByOcean ? _boardA[0] : _boardA[1];
		_oceanSpirit.Given_IsOn( _gameState.Tokens[oceanSpace] );

		// When: Tidal Boon is played (by Ocean)
		_gameState.Phase = Phase.Slow;
		_oceanSpirit.AddActionFactory( PowerCard.ForDecorated(TidalBoon.ActAsync) );
		Task task = _oceanSpirit.SelectAndResolveActions( _gameState );

		_oceanSpirit.NextDecision().ChooseFirstThatStartsWith( TidalBoon.Name );
		//  And: Pushes town into ocean
		NextDecision.ChooseFrom("T@2").ChooseTo("A0","A0,A1,A3,A4");// Choose( "T@2" ); NextDecision.HasOptions( "A0,A1,A3,A4" ).Choose( "A0" );

		// When: Pushes 1st Dahan into Ocean
		NextDecision.ChooseFrom("D@2").ChooseTo("A0","A0,A1,A3,A4");// Choose( "D@2" ); Choose( "A0" );

		// Thunderspeaker goes along
		Choose( "Ts" );

		if(savedByOcean) {
			// Ocean should decide if it is going to save them now
			_oceanSpirit.NextDecision()
				.HasPrompt("Save Dahan from Drowning")
				.HasOptions("D@2 on A0 => A1,D@2 on A0 => A2,D@2 on A0 => A3,D@2 on A0 => B1,D@2 on A0 => B2,D@2 on A0 => B3,Done")
				.Choose("D@2 on A0 => A1");
		}

		// When: Pushes 2nd dahan into Ocean
		NextDecision.ChooseFrom( "D@2" ).ChooseTo( "A0" );

		if(savedByOcean) {
			// Ocean should decide if it is going to save them now
			_oceanSpirit.NextDecision()
				.HasOptions("D@2 on A0 => A1,D@2 on A0 => A2,D@2 on A0 => A3,D@2 on A0 => B1,D@2 on A0 => B2,D@2 on A0 => B3,Done")
				.Choose("D@2 on A0 => A1");

			// End of Action - Thunder speaker exits ocean
			Choose( "Ts" );
		}

		await task.ShouldComplete();

		// Then: Ocean gets 1 energy (2 health / 2 players = 1 energy)
		_oceanSpirit.Energy.ShouldBe( 1 );

	}

	#region private helper methods

	void Given_OceanOnPrimaryBoard() => _oceanSpirit.Given_IsOn( _gameState.Tokens[_boardA[0]] ); // put ocean presence on primary's board, but not in the ocean

	void Given_PrimaryPresenceOnA2Only() => Given_PrimaryPresenceOnlyOn( _boardA[2] );

	void Given_PrimaryPresenceOnlyOn( SpaceSpec space ) {
		foreach(Space ss in _primarySpirit.Presence.Lands.ToArray())
			_primarySpirit.Given_IsOn( ss, 0 );

		// Add to
		_primarySpirit.Given_IsOn( _gameState.Tokens[space] );
		_primarySpirit.Presence.Lands.SelectLabels().Join( "," ).ShouldBe( space.Label );
	}

	
	void Choose( string text ) => NextDecision.Choose( text );

	static void IsActive( Task task ) => task.IsCompleted.ShouldBeFalse();

	#endregion

}
