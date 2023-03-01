namespace SpiritIsland.Tests;

public class AbsoluteStasis_Tests {

	readonly ConfigurableTestFixture cfg = new ConfigurableTestFixture();

	public AbsoluteStasis_Tests() {
		ActionScope.Initialize();
	}

	[Trait("SpecialRule","OceanInPlay")]
	[Fact]
	public async Task CannotTargetInPlayOceans() {
		// Given: Ocean is in play
		cfg.Spirit = new Ocean();
		//   And: Ocean presence is in ocean
		cfg.InitPresence(cfg.Board[0],2);

		cfg.GameState.Initialize();

		{
			await using var scope = await ActionScope.Start(ActionCategory.Spirit_Power);
			var selfCtx = cfg.Spirit.BindMyPowers();

			//  When: targeting with other card
			Task mesmerizedTranquilityTask = PowerCard.For<MesmerizedTranquility>()
				.ActivateAsync( selfCtx );
			//  Then: Ocean is an option
			mesmerizedTranquilityTask.IsCompleted.ShouldBeFalse();
			cfg.FormatOptions.ShouldContain( "A0" );
			cfg.Choose("A0");
			mesmerizedTranquilityTask.Wait(5);
			mesmerizedTranquilityTask.IsCompletedSuccessfully.ShouldBeTrue();
		}

		{
			// But...
			// When: targetting with Absolute Statis
			Task absoluteStasisTask = PowerCard.For<AbsoluteStasis>()
				.ActivateAsync( cfg.SelfCtx );
			// Then: Ocean is NOT an option (nothing to select)
			cfg.FormatOptions.ShouldNotContain("A0");
			cfg.Choose( "A1" );
			absoluteStasisTask.Wait( 5 );
			absoluteStasisTask.IsCompletedSuccessfully.ShouldBeTrue();
		}
	}

	[Trait( "Space", "State" )]
	[Trait( "Targeting", "Destinatoin" )]
	[Fact]
	public void CannotTargetIntoStasisSpace() {
		SpiritIs_FacturedDaysSplitTheSky();
		// Given: a spaces is put into stasis

		var stasisTask = PowerCard.For<AbsoluteStasis>()
			.ActivateAsync(cfg.SelfCtx);
		cfg.FormatOptions.ShouldBe("A1,A4,A5,A6,A7,A8");
		cfg.Choose("A5");
		stasisTask.Wait( 5 );
		stasisTask.IsCompletedSuccessfully.ShouldBeTrue();

		//  When: targetting a second card
		_ = PowerCard.For<DevouringAnts>()
			.ActivateAsync(cfg.SelfCtx);
		//  Then: stasis space is not an option.
		cfg.FormatOptions.ShouldBe( "A6,A7,A8" );

		cfg.Choose("A8"); // cleanup
	}

	[Trait( "Space", "State" )]
	[Trait( "Space", "Connectivity" )]
	[Trait( "Targeting", "Range" )]
	[Fact]
	public void CannotTargetThroughStasisSpace() {
		SpiritIs_FacturedDaysSplitTheSky();
		// Given: a spaces is put into stasis
		var stasisTask = PowerCard.For<AbsoluteStasis>()
			.ActivateAsync( cfg.SelfCtx );
		cfg.FormatOptions.ShouldBe( "A1,A4,A5,A6,A7,A8" );
		cfg.Choose( "A5" );
		stasisTask.Wait( 5 );
		stasisTask.IsCompletedSuccessfully.ShouldBeTrue();

		//  When: targetting a second card
		_ = PowerCard.For<PillarOfLivingFlame>()
			.ActivateAsync( cfg.SelfCtx );
		//  Then: stasis space is not an option.
		cfg.FormatOptions.ShouldBe( "A1,A6,A7,A8" );

		cfg.Choose( "A8" );
	}

	// cannot target out of Stasis space
	[Trait( "Space", "State" )]
	[Trait( "Targeting", "Source" )]
	[Fact]
	public void CannotTargetOutOfStasisSpace() {
		SpiritIs_FacturedDaysSplitTheSky();

		// Given: the only SS is put into stasis
		SpacePutInStasis( "A8" );

		//  When: targetting a second card
		var task2 = PowerCard.For<PillarOfLivingFlame>()
			.ActivateAsync( cfg.SelfCtx );
		//  Then: action completes - no source to target from
		task2.Wait( 5 );
		task2.IsCompletedSuccessfully.ShouldBeTrue();
	}

	// No Presence / Sacred Site in Stasis space
	[Trait( "Space", "State" )]
	[Fact]
	public void PresenceInStasis() {
		SpiritIs_FacturedDaysSplitTheSky();

		// When: the only SS is put into stasis
		SpacePutInStasis("A8");

		//  Then: no Presence found in A8
		cfg.SelfCtx.Self.Presence.Spaces.Count( x => x.Text == "A8" ).ShouldBe( 0 );
		//   And: no SS found
		cfg.Presence.SacredSites.Downgrade().Count( x => x.Text == "A8" ).ShouldBe( 0 );
	}

	// Invaders Don't Explore Into / out of
	[Trait( "Space", "State" )]
	[Trait( "Invaders", "Explore" )]
	[Fact]
	public void NotASourceOfInvadersExploring() {
		SpiritIs_FacturedDaysSplitTheSky();
		var destination = cfg.Board[7];
		var source = cfg.Board[8];

		// Given: Town on A8
		cfg.InitTokens( source, "1T@2");
		// And: source is in stasis
		SpacePutInStasis( source.Text );

		// When: Invaders Explore - destination
		destination.DoAnExplore(cfg.GameState).Wait(8);

		//  Then: no explorers in Jungle spaces.
		cfg.GameState.Tokens[destination].OfHumanClass(Human.Explorer).Length.ShouldBe( 0 );
	}

	// Invaders Don't Explore Into / out of
	[Trait( "Space", "State" )]
	[Trait( "Invaders", "Explore" )]
	[Fact]
	public void NotADestinationOfInvadersExploring() {
		SpiritIs_FacturedDaysSplitTheSky();
		var destination = cfg.Board[7];
		var source = cfg.Board[8];

		// Given: Town on A8
		cfg.InitTokens( source, "1T@2" );
		// And: Dst is in stasis
		SpacePutInStasis( destination.Text );

		// When: Invaders Explore - destination
		destination.DoAnExplore( cfg.GameState ).Wait( 8 );

		//  Then: no explorers in Jungle spaces.
		cfg.GameState.Tokens[destination].OfHumanClass( Human.Explorer ).Length.ShouldBe( 0 );
	}

	// Invaders Don't Build
	[Trait( "Space", "State" )]
	[Trait( "Invaders", "Build" )]
	[Fact]
	public void NoBuild() {
		SpiritIs_FacturedDaysSplitTheSky();
		var space = cfg.Board[7];

		// Given: Explorer on space
		cfg.InitTokens( space, "1E@1" );
		// And: space is in stasis
		SpacePutInStasis( space.Text );
		Assert_SpaceHasCountTokens(space, Human.Town, 0 );

		// When: Invaders Build
		space.DoABuild( cfg.GameState ).Wait( 8 );

		//  Then: no explorers in Jungle spaces.
		Assert_SpaceHasCountTokens( space, Human.Town, 0 );
	}

	// M Invaders Don't Ravage
	[Trait( "Space", "State" )]
	[Trait( "Invaders", "Ravage" )]
	[Fact]
	public void NoRavage() {
		SpiritIs_FacturedDaysSplitTheSky();
		var space = cfg.Board[7];

		// Given: 1 Explorer and 1 Dahan on space
		cfg.InitTokens( space, "1E@1,1D@2" );
		// And: space is in stasis
		SpacePutInStasis( space.Text );
		Assert_SpaceHasCountTokens( space, Human.Town, 0 );

		// When: Invaders Ravage
		new RavageSlot().ActivateCard( space.BuildInvaderCard(), GameState.Current ).Wait();

		//  Then: explorers and dahan unchanged
		Assert_SpaceHasCountTokens( space, Human.Explorer, 1 );
	}

	// stasis space is included in win/loss check
	[Trait( "Space", "State" )]
	[Trait( "Feature", "Win/Loss")]
	[Fact]
	public void RecognizesPresenceForWinLoss() {
		cfg.Spirit = new FracturedDaysSplitTheSky(); // don't call SpiritIs_ because it adds presence we don't want
		var space = cfg.Board[7];
		var destroyPresenceSpace =cfg.Board[3];


		// Given: Sacred Site on a space (so we can use Absolute Stasis Card)
		cfg.InitPresence(space,2);
		//   And: presence somewhere else we can destory
		cfg.InitPresence( destroyPresenceSpace, 1 );
		//   And: 1 city (so we don't win with 0 invaders)
		cfg.InitTokens( space, "1C@3");
		//   And: space is in stasis
		SpacePutInStasis( space.Text );
		//   And: should check win/loss
		cfg.GameState.AddStandardWinLossCheck();

		try{
			//  When: destroy that extra presence (triggers win/loss check)
			destroyPresenceSpace.Tokens.Destroy(cfg.Presence.Token,1).Wait();
			//  When: we check win/loss
			cfg.GameState.CheckWinLoss();
		} catch( GameOverException ) {
			// Then: no game-over exception is thrown and we get here
			throw new ShouldAssertException( "CheckWinLoss should not have thrown GameOverException." );
		}

	}

	[Trait( "Space", "State" )]
	[Fact]
	public void StillVisibleInAllSpaces() {
		SpiritIs_FacturedDaysSplitTheSky();
		var space = cfg.Board[7];

		// When: space is put in stasis
		SpacePutInStasis( space.Text );

		// Then: space still apears in list of All Spaces
		cfg.GameState.Spaces_Unfiltered.ShouldContain( cfg.GameState.Tokens[space] );

	}

	// Additional possible tests:

	// E Cannot Pull/Move From nor Push/Cascade/Move Into Stasis Space
	// Remove all calls to Space.Adjacent
	// Remove all calls to Space.Range(.)

	// H cannot destroy presence in Stasis space
	// Grinning Trickster removing blight
	// Thunderspeaker when Dahan destroyed

	// when round is over, space and tokens are restored

	void SpacePutInStasis( string spaceToStasisize ) {
		var stasisTask = PowerCard.For<AbsoluteStasis>()
			.ActivateAsync( cfg.SelfCtx );
		stasisTask.IsCompleted.ShouldBeFalse();
		cfg.FormatOptions.ShouldBe( "A1,A4,A5,A6,A7,A8" );
		cfg.Choose( spaceToStasisize );
		stasisTask.Wait( 5 );
		stasisTask.IsCompletedSuccessfully.ShouldBeTrue();
	}

	void SpiritIs_FacturedDaysSplitTheSky() {
		cfg.Spirit = new FracturedDaysSplitTheSky();
		cfg.GameState.MinorCards = new PowerCardDeck( typeof( RiversBounty ).GetMinors(), 0 );
		cfg.GameState.MajorCards = new PowerCardDeck( typeof( RiversBounty ).GetMajors(), 0 );
		cfg.GameState.Initialize();
		cfg.Presence.SacredSites.Select( x => x.Space.Text ).Join( "," ).ShouldBe( "A8" );
	}

	void Assert_SpaceHasCountTokens( Space space, IEntityClass tokenClass, int expectedCount ) {
		cfg.GameState.Tokens[space].OfClass( tokenClass ).Length.ShouldBe( expectedCount );
	}

}
