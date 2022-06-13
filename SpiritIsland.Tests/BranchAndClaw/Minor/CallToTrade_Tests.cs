namespace SpiritIsland.Tests.BranchAndClaw.Minor;

public class CallToTrade_Tests {

	// for Terror Level 2 or lower => "first Ravage in Target land becomes a build"
	[Trait( "Feature", "Ravage" )]
	[Trait( "Feature", "Build" )]
	[Trait( "Feature", "InvaderCardProgression" )]
	[Fact]
	public void NoRavage_NoBuild() {

		var (user, ctx) = TestSpirit.SetupGame( PowerCard.For<CallToTrade>() );

		// Given: a space that is not part of the build nor ravage
		var spaceCtx = ctx.AllSpaces
			.Select( ctx.Target )
			.Last( s => !s.MatchesBuildCard && !s.MatchesRavageCard ); // last stays away from city and ocean

		Given_HasOnly3Explorers( spaceCtx );
		Given_Has2Dahan( spaceCtx );
		Given_NoSuroundingTowns( spaceCtx );
		Given_NoSuroundingDahan( spaceCtx );

		When_GrowsBuysAndActivatesCard( user, spaceCtx );

		// Then: Card did not create a ravage there (nor a build)
		spaceCtx.Tokens.InvaderSummary().ShouldBe( "3E@1" );

	}

	[Trait( "Feature", "Ravage" )]
	[Trait( "Feature", "Build" )]
	[Trait( "Feature", "InvaderCardProgression" )]
	[Trait("Feature","Gather")]
	[Fact]
	public void OneRavage_ReplacedWithBuild() {
		var (user, ctx) = TestSpirit.SetupGame( PowerCard.For<CallToTrade>() );

		// Given: advance to 2nd round where we have a ravage
		user.DoesNothingForARound();

		// Given: a space that IS-RAVAGE but NOT-BUILD
		var spaceCtx = ctx.AllSpaces
			.Select( ctx.Target )
			.Last( s => s.MatchesRavageCard && !s.MatchesBuildCard ); // last stays away from city and ocean
																		//  And: it has 3 explorers (in case dahan attacks during ravage, would still 1 left over
		Given_HasOnly3Explorers( spaceCtx );
		Given_Has2Dahan( spaceCtx );
		Given_NoSuroundingTowns( spaceCtx );
		Given_NoSuroundingDahan( spaceCtx );

		string info = $"{spaceCtx.Space.Label} {ctx.GameState.InvaderDeck.Build.Single()} ";


		When_GrowsBuysAndActivatesCard( user, spaceCtx );

		// Then: Card converted ravage to a build
		spaceCtx.Tokens.InvaderSummary().ShouldBe( "1T@2,3E@1" );

	}

	[Trait( "Feature", "Ravage" )]
	[Trait( "Feature", "Build" )]
	[Trait( "Feature", "InvaderCardProgression" )]
	[Trait("Feature","Gather")]
	[Fact]
	public void TerrorLevel3_RavageRemainsRavage() {
		var (user, ctx) = TestSpirit.SetupGame( PowerCard.For<CallToTrade>() );

		// Elevate to Terror Level 3
		Given_TerrorLevelIs3( ctx );

		// Given: advance to 2nd round where we have a ravage
		user.DoesNothingForARound();

		// Given: a space that IS-RAVAGE but NOT-BUILD
		var spaceCtx = ctx.AllSpaces
			.Select( ctx.Target )
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

	[Trait( "Feature", "Ravage" )]
	[Trait( "Feature", "Build" )]
	[Trait( "Feature", "InvaderCardProgression" )]
	[Trait("Feature","Gather")]
	[Fact]
	public void BuildAndRavage_BecomesTwoBuilds() {

		List<string> invaderLog = new List<string>();

		// Given: Going to Ravage / Build in Jungle
		var (user, ctx) = TestSpirit.SetupGame( PowerCard.For<CallToTrade>(), (gs)=>{ 
			var jungleCard = InvaderCard.Stage1( Terrain.Jungle);
			gs.InvaderDeck = InvaderDeck.BuildTestDeck( jungleCard, jungleCard, jungleCard, jungleCard );
			gs.NewLogEntry += (s) => invaderLog.Add(s.Msg());
		} );

		// Given: advance to 2nd round where we have a ravage
		user.DoesNothingForARound();
		invaderLog.Clear();

		// Given: a space that IS-RAVAGE AND BUILD
		var spaceCtx = ctx.AllSpaces
			.Select( ctx.Target )
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

	// TwoRavages_BecomesBuildAndRavage - !!! how do I construct 2 ravages?

	#region Given / When

	static void Given_TerrorLevelIs3( SelfCtx ctx ) {
		var fear = ctx.GameState.Fear;
		for(int i = 0; i < 7; ++i)
			fear.Deck.Pop();
		fear.TerrorLevel.ShouldBe(3);
	}

	static void Given_NoSuroundingTowns( TargetSpaceCtx spaceCtx ) {
		// Simplifies power card because it has a Gather-Towns we don't want to deal with.
		foreach(var adj in spaceCtx.Adjacent)
			spaceCtx.Target( adj ).Tokens.InitDefault(Invader.Town, 0);
	}

	static void Given_NoSuroundingDahan( TargetSpaceCtx spaceCtx ) {
		// Simplifies power card because it has a Gather-dahan we don't want to deal with.
		foreach(var adj in spaceCtx.Adjacent) {
			var dahan = spaceCtx.Target( adj ).Tokens.Dahan;
			dahan.Init( 0 );
		}
	}

	static void Given_Has2Dahan( TargetSpaceCtx spaceCtx ) {
		spaceCtx.Dahan.Init( 2 );
	}

	static void Given_HasOnly3Explorers( TargetSpaceCtx spaceCtx ) {
		//  And: it has 3 explorers (in case dahan attacks during ravage, would still 1 left over
		spaceCtx.Tokens.InitDefault( Invader.Explorer, 3 );
		spaceCtx.Tokens.InitDefault( Invader.Town    , 0 );
		spaceCtx.Tokens.InitDefault( Invader.City    , 0 ); // if we had to advance cards, might have buit a city
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