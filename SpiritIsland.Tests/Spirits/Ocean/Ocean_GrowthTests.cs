namespace SpiritIsland.Tests.Spirits.OceanNS; 

[Collection("BaseGame Spirits")]
public class Ocean_GrowthTests : BoardAGame {

	public Ocean_GrowthTests():base( new Ocean() ) {}

	[Theory]
	[InlineData("A0","","A0:1",0)]
	[InlineData("A0B0","","A0:1,B0:1",0)]
	[InlineData("A0B0C0","","A0:1,B0:1,C0:1",0)]
	[InlineData("A1","A1>A0","A0:1",1)]
	[InlineData("A1B1","A1>A0,B1>B0","A0:1,B0:1",2)]
	[InlineData("A1B1C1","A1>A0,B1>B0,C1>C0","A0:1,B0:1,C0:1",3)]
	[InlineData("A1A2","A1>A0","A0:1,A2:1",1)]    // need to define which presence to move
	[InlineData("A1A2","A2>A0","A0:1,A1:1",1)]    // need to define which presence to move
	[InlineData("A1A2B1C1C2","A2>A0,B1>B0,C1>C0","A0:1,A1:1,B0:1,C0:1,C2:1",3)]    // need to define which presence to move
	public void ReclaimGather_GatherParts(string starting, string select, string ending, int gatherCounts) {
		Given_IslandIsABC();
		_spirit.Given_IsOnMany( starting );

		foreach(IHelpGrowActionFactory action in _spirit.GrowthTrack.Groups[0].GrowthActionFactories.Where(x=>!x.AutoRun))
			_spirit.AddActionFactory( action );

		// since options are move source, key on that
		var moveBySrc = select.Split(',')
			.Where(x=>!string.IsNullOrEmpty(x))
			.Select(s=>s.Split('>'))
			.ToDictionary(a=>"OHG on "+a[0],a=>a[1]);

		IHelpGrowActionFactory gather = _spirit.GetAvailableActions(Phase.Growth)
			.OfType<GrowthAction>()
			.Where(x=>x.Cmd.Description == "Gather 1 Presence into EACH Ocean" ) // is GatherPresenceIntoOcean
			.SingleOrDefault();

		if(gather != null){
			_ = gather.ActivateAsync( _spirit );
			while(0 < gatherCounts--) {
				var options = _spirit.Portal.Next.Options
					.Where( option => moveBySrc.ContainsKey( option.Text ) )
					.ToArray();
				var source = options.First();
				_spirit.Portal.Choose( _spirit.Portal.Next, source );
			}
		}

		// Then: nothing to gather
		_spirit.Assert_BoardPresenceIs( ending );
	}


	[Theory]
	[InlineData("A1A2")]    // need to define which presence to move
	public void ReclaimGather_GatherParts_Unresolved(string starting){

		// Given: 3-board island
		Given_IslandIsABC();

		_spirit.Given_IsOnMany( starting );

		// Changed implementation to not run unresolved things
	}

	[Fact]
	public async Task ReclaimGather_NonGatherParts() {
		// reclaim, +1 power, gather 1 presense into EACH ocean, +2 energy

		_spirit.Given_HalfOfHandDiscarded();

		await _spirit.When_Growing( 0, (user) => {
			user.Growth_DrawsPowerCard();
			user.SelectsMinorDeck();
			user.SelectMinorPowerCard();

			user.GathersPresenceIntoOcean();
		} );

		_spirit.Assert_AllCardsAvailableToPlay( 4 + 1 );
		Assert_GainsFirstMinorCard();
		_spirit.Assert_HasEnergy( 2 );
	}

	[Fact]
	public async Task TwoPresenceInOceans() {
		// +1 presence range any ocean, +1 presense in any ociean, +1 energy

		// Given: island has 2 boards, hence 2 oceans
		Given_IslandAB();

		await _spirit.When_Growing( 1, (user) => {
			user.PlacesPresenceInOcean( "Place in Ocean,[Place in Ocean]", "[moon energy],2 cardplay", "[A0],B0" );
			user.PlacesPresenceInOcean( "Place in Ocean", "[water energy],2 cardplay,OHG", "A0,[B0]" );
		} );

		_spirit.Assert_HasEnergy( 1 );
	}

	[Theory]
	[InlineData("A0","A1;A2;A3","A1:1,A2:1")]
	public async Task PowerPlaceAndPush( string starting, string placeOptions, string ending ) {
		// gain power card
		// push 1 presense from each ocean
		// add presense on coastal land range 1
		Given_IslandIsABC();
		_spirit.Given_IsOnMany( starting );

		await _spirit.When_Growing( 2, (user) => {
			user.Growth_PlacesEnergyPresence( placeOptions );
			user.Growth_DrawsPowerCard();
			user.SelectsMinorDeck();
			user.SelectMinorPowerCard();

//			user.PushesPresenceFromOcean( "A1,[A2],A3" );
			user.AssertDecisionInfo("Select Growth to resolve", "Push Presence from Ocean");
			user.NextDecision.HasPrompt("Select Presence to push").HasOptions("OHG on A0 => A1,OHG on A0 => A2,OHG on A0 => A3").Choose("OHG on A0 => A2");
		} );

		Assert_GainsFirstMinorCard();
		_spirit.Assert_BoardPresenceIs( ending );
	}

	[Trait("Presence","EnergyTrack")]
	[Theory]
	[InlineDataAttribute(1,0,"")]
	[InlineDataAttribute(2,0,"moon")]
	[InlineDataAttribute(3,0,"moon water")]
	[InlineDataAttribute(4,1,"moon water")]
	[InlineDataAttribute(5,1,"moon water earth")]
	[InlineDataAttribute(6,1,"moon 2 water earth")]
	[InlineDataAttribute(7,2, "moon 2 water earth" )]
	public Task EnergyTrack(int revealedSpaces, int expectedEnergyGrowth, string elements ) {
		var fixture = new ConfigurableTestFixture { Spirit = new Ocean() };
		return fixture.VerifyEnergyTrack( revealedSpaces, expectedEnergyGrowth, elements );
	}

	[Trait("Presence","CardTrack")]
	[Theory]
	[InlineDataAttribute(1,1)]
	[InlineDataAttribute(2,2)]
	[InlineDataAttribute(3,2)]
	[InlineDataAttribute(4,3)]
	[InlineDataAttribute(5,4)]
	[InlineDataAttribute(6,5)]
	public Task CardTrack( int revealedSpaces, int expectedCardPlayCount ) {
		var fixture = new ConfigurableTestFixture { Spirit = new Ocean() };
		return fixture.VerifyCardTrack( revealedSpaces, expectedCardPlayCount, "" );
	}

	[Trait("Spirit","SetupAction")]
	[Fact]
	public void HasSetUp() {
		var fxt = new ConfigurableTestFixture { Spirit = new Ocean() };
		fxt.GameState.Initialize();
		fxt.Spirit.GetAvailableActions( Phase.Init ).Count().ShouldBe( 1 );
	}

	#region private helper methos

	void Given_IslandIsABC() {
		// Given: 3-board island
		_gameState.Island = new Island( 
			BoardFactory.BuildA( GameBuilder.FourBoardLayout[0] ), 
			BoardFactory.BuildB( GameBuilder.FourBoardLayout[1] ), 
			BoardFactory.BuildC( GameBuilder.FourBoardLayout[2] )
		);
	}

	void Given_IslandAB() {
		_gameState.Island = new Island( 
			BoardFactory.BuildA( GameBuilder.FourBoardLayout[0] ), 
			BoardFactory.BuildB( GameBuilder.FourBoardLayout[1] )
		);
	}

	void Assert_GainsFirstMinorCard() {
		_spirit.Assert_HasCardAvailable( "Drought" );
	}

	#endregion

}

