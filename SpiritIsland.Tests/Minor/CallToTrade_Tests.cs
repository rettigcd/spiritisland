namespace SpiritIsland.Tests.Minor;

public class CallToTrade_Tests {

	static IEnumerable<TargetSpaceCtx> AllTargets( SelfCtx ctx ) {
		return ctx.GameState.Spaces_Unfiltered
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
		user.AdvancesToStartOfNextInvaderPhase();
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
		Given_TerrorLevelIs3( ctx );

		// Given: advance to 2nd round where we have a ravage
		user.AdvancesToStartOfNextInvaderPhase();
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
		user.AdvancesToStartOfNextInvaderPhase();
		invaderLog.Clear();

		// Given: a space that IS-RAVAGE AND BUILD
		var spaceCtx = AllTargets( ctx )
			.Last( s => s.MatchesRavageCard && s.MatchesBuildCard ); // last stays away from city and ocean
		invaderLog.Add("Selected target:"+spaceCtx.Space.Label );

		//  And: it has 3 explorers (in case dahan attacks during ravage, would still 1 left over
		Given_HasOnly3Explorers( spaceCtx );
		Given_Has2Dahan( spaceCtx );
		Given_NoSuroundingTowns( spaceCtx );
		Given_NoSuroundingDahan( spaceCtx );

		When_GrowsBuysAndActivatesCard( user, spaceCtx );

		spaceCtx.Tokens.InvaderSummary().ShouldBe( "1C@3,1T@2,4E@1" );
	}

	// TwoRavages_BecomesBuildAndRavage - !!! Put 2 cards in the Ravage slot that trigger the same space. Show only 1 ravage happens

	#region Given / When

	static void Given_TerrorLevelIs3( SelfCtx ctx ) {
		var fear = ctx.GameState.Fear;
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