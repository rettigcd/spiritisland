namespace SpiritIsland.Tests.Minor;

public class CallToTrade_Tests {

	static IEnumerable<TargetSpaceCtx> AllTargets( SelfCtx ctx ) {
		return GameState.Current.Spaces_Unfiltered
			.Select( s => ctx.Target(s.Space) );
	}

	// for Terror Level 2 or lower => "first Ravage in Target land becomes a build"
	[Trait( "Invaders", "Ravage" )]
	[Trait( "Invaders", "Build" )]
	[Trait( "Invaders", "Deck" )]
	[Fact]
	public void NoRavage_NoBuild() {

		var (user, ctx) = TestSpirit.StartGame( PowerCard.For<CallToTrade>() );

		// Given: a space that is not part of the build nor ravage
		var spaceCtx = AllTargets( ctx )
			.Last( s => !s.MatchesBuildCard && !s.MatchesRavageCard ); // last stays away from city and ocean

		Given_HasOnly3Explorers( spaceCtx );
		Given_Has2Dahan( spaceCtx );
		Given_NoSuroundingTowns( spaceCtx );
		Given_NoSuroundingDahan( spaceCtx );

		When_GrowsBuysAndActivatesCard( user, spaceCtx );

		// Then: Card did not create a ravage there (nor a build)
		spaceCtx.Tokens.InvaderSummary().ShouldBe( "3E@1" );

	}

	[Trait( "Invaders", "Ravage" )]
	[Trait( "Invaders", "Build" )]
	[Trait( "Invaders", "Deck" )]
	[Trait( "Feature","Gather" )]
	[Fact]
	public void OneRavage_ReplacedWithBuild() {
		var (user, ctx) = TestSpirit.StartGame( PowerCard.For<CallToTrade>() );

		// Given: advance to 2nd round where we have a ravage
		user.GrowAndBuyNoCards();
		user.WaitForNext();

		// Given: a space that IS-RAVAGE but NOT-BUILD
		var spaceCtx = AllTargets( ctx )
			.Last( s => s.MatchesRavageCard && !s.MatchesBuildCard ); // last stays away from city and ocean

		//  And: it has 3 explorers (in case dahan attacks during ravage, would still 1 left over
		Given_HasOnly3Explorers( spaceCtx );
		Given_Has2Dahan( spaceCtx );
		Given_NoSuroundingTowns( spaceCtx );
		Given_NoSuroundingDahan( spaceCtx );

		When_GrowsBuysAndActivatesCard( user, spaceCtx );
		user.WaitForNext();

		// Then: Card converted ravage to a build
		spaceCtx.Tokens.InvaderSummary().ShouldBe( "1T@2,3E@1" );

	}

	[Trait( "Invaders", "Ravage" )]
	[Trait( "Invaders", "Build" )]
	[Trait( "Invaders", "Deck" )]
	[Trait("Feature","Gather")]
	[Fact]
	public void TerrorLevel3_RavageRemainsRavage() {
		var (user, ctx) = TestSpirit.StartGame( PowerCard.For<CallToTrade>() );

		// Elevate to Terror Level 3
		Given_TerrorLevelIs3();

		// Given: advance to 2nd round where we have a ravage
		user.GrowAndBuyNoCards();
		user.WaitForNext();

		// Given: a space that IS-RAVAGE but NOT-BUILD
		var spaceCtx = AllTargets( ctx )
			.Last( s => s.MatchesRavageCard && !s.MatchesBuildCard ); // last stays away from city and ocean
																		//  And: it has 3 explorers (in case dahan attacks during ravage, would still 1 left over
		Given_HasOnly3Explorers( spaceCtx );
		Given_Has2Dahan( spaceCtx );
		Given_NoSuroundingTowns( spaceCtx );
		Given_NoSuroundingDahan( spaceCtx );

		When_GrowsBuysAndActivatesCard( user, spaceCtx );

		// Then: Ravage remains - 3 explorers kill 1 dahan, remaining dahan kills 2 explorers
		user.WaitForNext();
		spaceCtx.Tokens.InvaderSummary().ShouldBe( "1E@1" );
	}

	[Trait( "Invaders", "Ravage" )]
	[Trait( "Invaders", "Build" )]
	[Trait( "Invaders", "Deck" )]
	[Trait("Feature","Gather")]
	[Fact]
	public void BuildAndRavage_BecomesTwoBuilds() {

		List<string> invaderLog = new List<string>();

		// Given: Going to Ravage / Build in Jungle
		var (user, ctx) = TestSpirit.StartGame( PowerCard.For<CallToTrade>(), (Action<GameState>)((gs)=>{ 
			var jungleCard = SpiritIsland.InvaderCard.Stage1( Terrain.Jungle);
			gs.InitTestInvaderDeck( (InvaderCard)jungleCard, (InvaderCard)jungleCard, (InvaderCard)jungleCard, (InvaderCard)jungleCard );
			gs.NewLogEntry += (s) => invaderLog.Add( s.Msg());
		}) );

		// Given: advance to 2nd round where we have a ravage
		user.GrowAndBuyNoCards();
		invaderLog.Clear();

		// Given: a space that IS-RAVAGE AND BUILD
		user.WaitForNext();
		var spaceCtx = AllTargets( ctx )
			.Last( s => s.MatchesRavageCard && s.MatchesBuildCard ); // last stays away from city and ocean
		invaderLog.Add("Selected target:"+spaceCtx.Space.Label );

		//  And: it has 3 explorers (in case dahan attacks during ravage, would still 1 left over
		Given_HasOnly3Explorers( spaceCtx );
		Given_Has2Dahan( spaceCtx );
		Given_NoSuroundingTowns( spaceCtx );
		Given_NoSuroundingDahan( spaceCtx );

		When_GrowsBuysAndActivatesCard( user, spaceCtx );

		user.WaitForNext();
		spaceCtx.Tokens.InvaderSummary().ShouldBe( "1C@3,1T@2,4E@1" );
	}

	[Fact]
	public void TwoRavages_BecomesBuildAndRavage() {

		// Given: Going to Ravage / Build in Jungle
		var (user, ctx) = TestSpirit.StartGame( PowerCard.For<CallToTrade>(), (Action<GameState>)(( gs ) => {
			var jungleCard = SpiritIsland.InvaderCard.Stage1( Terrain.Jungle );
			gs.InitTestInvaderDeck( (InvaderCard)jungleCard, (InvaderCard)jungleCard, (InvaderCard)jungleCard, (InvaderCard)jungleCard );
		}) );
		var invaderLog = GameState.Current.LogAsStringList();

		// Given: advance to 2nd round where we have a ravage
		user.GrowAndBuyNoCards();
		invaderLog.Clear();

		// Given: a space that IS RAVAGE
		user.WaitForNext();
		var spaceCtx = AllTargets( ctx ).Last( s => s.MatchesRavageCard && s.MatchesBuildCard ); // last stays away from city and ocean
		invaderLog.Add( "Selected target:" + spaceCtx.Space.Label );
		//   And: there are 2 ravages for that space
		List<InvaderCard> ravageCards = GameState.Current.InvaderDeck.Ravage.Cards; ravageCards.Add( ravageCards[0] );

		//  And: it has 3 explorers and 2 dahan (in case dahan attacks during ravage, would still 1 left over
		spaceCtx.Space.Given_ClearTokens().Given_HasTokens("3E@1,2D@2");
		Given_NoSuroundingTowns( spaceCtx );
		Given_NoSuroundingDahan( spaceCtx );

		// When:
		When_GrowsBuysAndActivatesCard( user, spaceCtx );
		user.WaitForNext();

		// Then: it ravaged => 1E@1, 1D@2, then built => 1
		spaceCtx.Tokens.Summary.ShouldBe( "1B,1C@3,1D@2,2E@1,1T@2" );
	}

	#region Given / When

	static void Given_TerrorLevelIs3() {
		var fear = GameState.Current.Fear;
		for(int i = 0; i < 7; ++i)
			fear.Deck.Pop();
		fear.TerrorLevel.ShouldBe(3);
	}

	static void Given_NoSuroundingTowns( TargetSpaceCtx spaceCtx ) {
		// Simplifies power card because it has a Gather-Towns we don't want to deal with.
		foreach(var adjState in spaceCtx.Adjacent)
			adjState.InitDefault( Human.Town, 0 );
	}

	static void Given_NoSuroundingDahan( TargetSpaceCtx spaceCtx ) {
		// Simplifies power card because it has a Gather-dahan we don't want to deal with.
		foreach(var adjState in spaceCtx.Adjacent)
			adjState.Dahan.Init( 0 );
	}

	static void Given_Has2Dahan( TargetSpaceCtx spaceCtx ) {
		spaceCtx.Dahan.Init( 2 );
	}

	static void Given_HasOnly3Explorers( TargetSpaceCtx spaceCtx ) {
		//  And: it has 3 explorers (in case dahan attacks during ravage, would still 1 left over
		spaceCtx.Tokens.InitDefault( Human.Explorer, 3 );
		spaceCtx.Tokens.InitDefault( Human.Town    , 0 );
		spaceCtx.Tokens.InitDefault( Human.City    , 0 ); // if we had to advance cards, might have buit a city
		spaceCtx.Tokens.InvaderSummary().ShouldBe( "3E@1", "Unable to init to 3 exploreres." );
	}


	static void When_GrowsBuysAndActivatesCard( VirtualTestUser user, TargetSpaceCtx spaceCtx ) {
		// When: grows and purchases card
		user.Grows();
		user.PlaysCard( CallToTrade.Name );
		//  And: Activates Card
		user.SelectsFastAction( CallToTrade.Name );
		user.TargetsLand_IgnoreOptions( spaceCtx.Space.Label );
	}

	#endregion

}