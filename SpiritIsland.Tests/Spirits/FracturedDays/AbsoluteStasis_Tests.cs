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
			await cfg.Spirit.When_ResolvingCard<MesmerizedTranquility>( (user) => {
				//  Then: Ocean is an option
				cfg.FormatOptions.ShouldContain( "A0" );
				user.Choose( "A0" );
			} );
		}

		{
			// But...
			// When: targetting with Absolute Statis
			await cfg.Spirit.When_ResolvingCard<AbsoluteStasis>( (user) => {
				// Then: Ocean is NOT an option (nothing to select)
				cfg.FormatOptions.ShouldNotContain( "A0" );
				user.Choose( "A1" );
			} );
		}
	}

	[Trait( "Space", "State" )]
	[Trait( "Targeting", "Destinatoin" )]
	[Fact]
	public async Task CannotTargetIntoStasisSpace() {
		SpiritIs_FacturedDaysSplitTheSky();

		// Given: a spaces is put into stasis
		await Given_SpacePutInStasis( "A5" );

		//  When: targetting a second card
		await cfg.Spirit.When_ResolvingCard<DevouringAnts>( (user) => {
			//  Then: stasis space is not an option.
			cfg.FormatOptions.ShouldBe( "A6,A7,A8" );
			user.Choose( "A8" );
			// cleanup
			user.Choose( "T@2" );
			user.Choose( "T@1" );
		} );
	}

	[Trait( "Space", "State" )]
	[Trait( "Space", "Connectivity" )]
	[Trait( "Targeting", "Range" )]
	[Fact]
	public async Task CannotTargetThroughStasisSpace() {
		SpiritIs_FacturedDaysSplitTheSky();

		// Given: a spaces is put into stasis
		await cfg.Spirit.When_ResolvingCard<AbsoluteStasis>( (user) => { 
			cfg.FormatOptions.ShouldBe("A1,A4,A5,A6,A7,A8"); 
			user.Choose("A5"); 
		} );

		//  When: targetting a second card
		await cfg.Spirit.When_ResolvingCard<PillarOfLivingFlame>((user)=> {
			//  Then: stasis space is not an option.
			user.NextDecision.HasOptions( "A1,A6,A7,A8" ).Choose( "A8" );
			// cleanup 
			user.Choose( "T@2" );
			user.Choose( "T@1" );
			user.Choose( "E@1" );
		} );
	}

	// cannot target out of Stasis space
	[Trait( "Space", "State" )]
	[Trait( "Targeting", "Source" )]
	[Fact]
	public async Task CannotTargetOutOfStasisSpace() {
		SpiritIs_FacturedDaysSplitTheSky();

		// Given: the only SS is put into stasis
		await Given_SpacePutInStasis( "A8" );

		//  When: targetting a second card
		await PowerCard.For(typeof(PillarOfLivingFlame)).ActivateAsync( cfg.Spirit )
			//  Then: action completes - no source to target from
			.ShouldComplete(PillarOfLivingFlame.Name);
	}

	// No Presence / Sacred Site in Stasis space
	[Trait( "Space", "State" )]
	[Fact]
	public async Task PresenceInStasis() {
		SpiritIs_FacturedDaysSplitTheSky();

		// When: the only SS is put into stasis
		await Given_SpacePutInStasis("A8");

		//  Then: no Presence found in A8
		cfg.Spirit.Presence.Lands.Count( x => x.Text == "A8" ).ShouldBe( 0 );
		//   And: no SS found
		cfg.Presence.SacredSites.Downgrade().Count( x => x.Text == "A8" ).ShouldBe( 0 );
	}

	// Invaders Don't Explore Into / out of
	[Trait( "Space", "State" )]
	[Trait( "Invaders", "Explore" )]
	[Fact]
	public async Task NotASourceOfInvadersExploring() {
		SpiritIs_FacturedDaysSplitTheSky();
		var destination = cfg.Board[7];
		var source = cfg.Board[8];

		// Given: Town on A8
		cfg.InitTokens( source, "1T@2");
		// And: source is in stasis
		await Given_SpacePutInStasis( source.Text );

		// When: Invaders Explore - destination
		await destination.When_Exploring();

		//  Then: no explorers in Jungle spaces.
		cfg.GameState.Tokens[destination].HumanOfTag(Human.Explorer).Length.ShouldBe( 0 );
	}

	// Invaders Don't Explore Into / out of
	[Trait( "Space", "State" )]
	[Trait( "Invaders", "Explore" )]
	[Fact]
	public async Task NotADestinationOfInvadersExploring() {
		SpiritIs_FacturedDaysSplitTheSky();
		var destination = cfg.Board[7];
		var source = cfg.Board[8];

		// Given: Town on A8
		cfg.InitTokens( source, "1T@2" );
		// And: Dst is in stasis
		await Given_SpacePutInStasis( destination.Text );

		// When: Invaders Explore - destination
		await destination.When_Exploring();

		//  Then: no explorers in Jungle spaces.
		cfg.GameState.Tokens[destination].HumanOfTag( Human.Explorer ).Length.ShouldBe( 0 );
	}

	// Invaders Don't Build
	[Trait( "Space", "State" )]
	[Trait( "Invaders", "Build" )]
	[Fact]
	public async Task NoBuild() {
		SpiritIs_FacturedDaysSplitTheSky();
		var space = cfg.Board[7];

		// Given: Explorer on space
		cfg.InitTokens( space, "1E@1" );
		// And: space is in stasis
		await Given_SpacePutInStasis( space.Text );
		Assert_SpaceHasCountTokens(space, Human.Town, 0 );

		// When: Invaders Build
		await space.When_Building();

		//  Then: no explorers in Jungle spaces.
		Assert_SpaceHasCountTokens( space, Human.Town, 0 );
	}

	// M Invaders Don't Ravage
	[Trait( "Space", "State" )]
	[Trait( "Invaders", "Ravage" )]
	[Fact]
	public async Task NoRavage() {
		SpiritIs_FacturedDaysSplitTheSky();
		var space = cfg.Board[7];

		// Given: 1 Explorer and 1 Dahan on space
		cfg.InitTokens( space, "1E@1,1D@2" );
		// And: space is in stasis
		await Given_SpacePutInStasis( space.Text );
		Assert_SpaceHasCountTokens( space, Human.Town, 0 );

		// When: Invaders Ravage
		await space.When_Ravaging();

		//  Then: explorers and dahan unchanged
		Assert_SpaceHasCountTokens( space, Human.Explorer, 1 );
	}

	// stasis space is included in win/loss check
	[Trait( "Space", "State" )]
	[Trait( "Feature", "Win/Loss")]
	[Fact]
	public async Task RecognizesPresenceForWinLoss() {
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
		await Given_SpacePutInStasis( space.Text );
		//   And: should check win/loss
		cfg.GameState.AddStandardWinLossCheck();

		try{
			//  When: destroy that extra presence (triggers win/loss check)
			await destroyPresenceSpace.Tokens.Destroy(cfg.Presence.Token,1).ShouldComplete("destroying presence");
			//  When: we check win/loss
			cfg.GameState.CheckWinLoss();
		} catch( GameOverException ) {
			// Then: no game-over exception is thrown and we get here
			throw new ShouldAssertException( "CheckWinLoss should not have thrown GameOverException." );
		}

	}

	[Trait( "Space", "State" )]
	[Fact]
	public async Task StillVisibleInAllSpaces() {
		SpiritIs_FacturedDaysSplitTheSky();
		var space = cfg.Board[7];

		// When: space is put in stasis
		await Given_SpacePutInStasis( space.Text );

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

	Task Given_SpacePutInStasis( string spaceToStasisize ) {
		return cfg.Spirit.When_ResolvingCard<AbsoluteStasis>( (user) => {
			cfg.FormatOptions.ShouldBe( "A1,A4,A5,A6,A7,A8" );
			user.Choose( spaceToStasisize );
		} );
	}

	void SpiritIs_FacturedDaysSplitTheSky() {
		cfg.Spirit = new FracturedDaysSplitTheSky();
		cfg.GameState.MinorCards = new PowerCardDeck( typeof( RiversBounty ).GetMinors(), 0 );
		cfg.GameState.MajorCards = new PowerCardDeck( typeof( RiversBounty ).GetMajors(), 0 );
		cfg.GameState.Initialize();
		cfg.Presence.SacredSites.Select( x => x.Space.Text ).Join( "," ).ShouldBe( "A8" );
	}

	void Assert_SpaceHasCountTokens( Space space, ITokenClass tokenClass, int expectedCount ) {
		cfg.GameState.Tokens[space].OfTag( tokenClass ).Length.ShouldBe( expectedCount );
	}

}
