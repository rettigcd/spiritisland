namespace SpiritIsland.Tests.Spirits.OceanNS; 

public class Ocean_GrowthTests : GrowthTests {

	public Ocean_GrowthTests():base( new Ocean() ) {
		ActionScope.Initialize();
	}

	[Theory]
	[InlineData("A0","","A0:1")]
	[InlineData("A0B0","","A0:1,B0:1")]
	[InlineData("A0B0C0","","A0:1,B0:1,C0:1")]
	[InlineData("A1","A1>A0","A0:1")]
	[InlineData("A1B1","A1>A0,B1>B0","A0:1,B0:1")]
	[InlineData("A1B1C1","A1>A0,B1>B0,C1>C0","A0:1,B0:1,C0:1")]
	[InlineData("A1A2","A1>A0","A0:1,A2:1")]    // need to define which presence to move
	[InlineData("A1A2","A2>A0","A0:1,A1:1")]    // need to define which presence to move
	[InlineData("A1A2B1C1C2","A2>A0,B1>B0,C1>C0","A0:1,A1:1,B0:1,C0:1,C2:1")]    // need to define which presence to move
	public void ReclaimGather_GatherParts(string starting, string select, string ending) {
		Given_IslandIsABC();
		Given_HasPresence( starting );

		_spirit.QueueUpGrowth(_spirit.GrowthTrack.Options[0]);

		// since options are move source, key on that
		var moveBySrc = select.Split(',')
			.Where(x=>!string.IsNullOrEmpty(x))
			.Select(s=>s.Split('>'))
			.ToDictionary(a=>a[0],a=>a[1]);

		GatherPresenceIntoOcean gather = _spirit.GetAvailableActions(Phase.Growth).OfType<GatherPresenceIntoOcean>().SingleOrDefault();

		if(gather != null){
			_ = gather.ActivateAsync( _spirit.BindSelf() );
			while(!_spirit.Gateway.IsResolved){
				var source = _spirit.Gateway.Next.Options.Single(x=>moveBySrc.ContainsKey(x.Text));
				_spirit.Gateway.Choose( _spirit.Gateway.Next, source );
			}
		}

		// Then: nothing to gather
		Assert_BoardPresenceIs( ending );
	}


	[Theory]
	[InlineData("A1A2")]    // need to define which presence to move
	public void ReclaimGather_GatherParts_Unresolved(string starting){

		// Given: 3-board island
		_gameState.Island = new Island(BoardA,BoardB,BoardC);

		Given_HasPresence( starting );

		// Changed implementation to not run unresolved things
	}

	[Fact]
	public void ReclaimGather_NonGatherParts() {
		// reclaim, +1 power, gather 1 presense into EACH ocean, +2 energy

		Given_HalfOfPowercardsPlayed();

		_spirit.When_Growing( 0, () => {
			User.Growth_DrawsPowerCard();
			User.SelectsMinorDeck();
			User.SelectMinorPowerCard();

			User.GathersPresenceIntoOcean();
		} );

		Assert_AllCardsAvailableToPlay( 4 + 1 );
		Assert_GainsFirstMinorCard();
		Assert_HasEnergy( 2 );
	}

	[Fact]
	public void TwoPresenceInOceans() {
		// +1 presence range any ocean, +1 presense in any ociean, +1 energy

		// Given: island has 2 boards, hence 2 oceans
		_gameState.Island = new Island( BoardA, BoardB );

		_spirit.When_Growing( 1, () => {
			User.PlacesPresenceInOcean( "PlaceInOcean,[PlaceInOcean]", "[moon energy],2 cardplay,Take Presence from Board", "[A0],B0" );
			User.PlacesPresenceInOcean( "PlaceInOcean", "[water energy],2 cardplay,Take Presence from Board", "A0,[B0]" );
		} );

		Assert_HasEnergy( 1 );
	}

	[Theory]
	[InlineData("A0","A1;A2;A3","A1:1,A2:1")]
	public void PowerPlaceAndPush( string starting, string placeOptions, string ending ) {
		// gain power card
		// push 1 presense from each ocean
		// add presense on coastal land range 1
		_gameState.Island = new Island( BoardA, BoardB, BoardC );
		Given_HasPresence( starting );

		_spirit.When_Growing( 2, () => {
			User.Growth_PlacesEnergyPresence( placeOptions );
			User.Growth_DrawsPowerCard();
			User.SelectsMinorDeck();
			User.SelectMinorPowerCard();

			User.PushesPresenceFromOcean( "A1,[A2],A3" );
		} );

		Assert_GainsFirstMinorCard();
		Assert_BoardPresenceIs( ending );
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
	public async Task EnergyTrack(int revealedSpaces, int expectedEnergyGrowth, string elements ) {// !!! still async
		var fixture = new ConfigurableTestFixture { Spirit = new Ocean() };
		await fixture.VerifyEnergyTrack( revealedSpaces, expectedEnergyGrowth, elements );
	}

	[Trait("Presence","CardTrack")]
	[Theory]
	[InlineDataAttribute(1,1)]
	[InlineDataAttribute(2,2)]
	[InlineDataAttribute(3,2)]
	[InlineDataAttribute(4,3)]
	[InlineDataAttribute(5,4)]
	[InlineDataAttribute(6,5)]
	public async Task CardTrack( int revealedSpaces, int expectedCardPlayCount ) {
		var fixture = new ConfigurableTestFixture { Spirit = new Ocean() };
		await fixture.VerifyCardTrack( revealedSpaces, expectedCardPlayCount, "" );
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
		_gameState.Island = new Island( BoardA, BoardB, BoardC );
	}

	void Assert_GainsFirstMinorCard() {
		Assert_HasCardAvailable( "Drought" );
	}

	#endregion

}

