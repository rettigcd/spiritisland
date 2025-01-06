namespace SpiritIsland.Tests.Spirits.FracturedDaysNS;

public class AbsoluteStasis_Tests {

	public AbsoluteStasis_Tests() {}

	[Trait("SpecialRule","OceanInPlay")]
	[Fact]
	public async Task CannotTargetInPlayOceans() {

		var gs = new SoloGameState(new Ocean(), Boards.A);

		//   And: Ocean presence is in ocean
		gs.Spirit.Given_IsOn(gs.Board[0].ScopeSpace, 2);

		gs.Initialize();

		{
			await gs.Spirit.When_ResolvingCard<MesmerizedTranquility>( (user) => {
				//  Then: Ocean is an option
				user.NextDecision.Choose("A0");
			} );
		}

		{
			// But...
			// When: targetting with Absolute Statis
			await gs.Spirit.When_ResolvingCard<AbsoluteStasis>( (user) => {
				// Then: Ocean is NOT an option (nothing to select)
				user.Choose( "A1" );
			} );
		}
	}

	[Trait( "Space", "State" )]
	[Trait( "Targeting", "Destinatoin" )]
	[Fact]
	public async Task CannotTargetIntoStasisSpace() {

		var spirit = SpiritIs_FacturedDaysSplitTheSky();
		Give_SpiritOnA8(spirit);


		// Given: a spaces is put into stasis
		await Given_SpacePutInStasis(spirit, "A5");

		//  When: targetting a second card
		await spirit.When_ResolvingCard<DevouringAnts>( (user) => {
			//  Then: stasis space is not an option.
			user.NextDecision.HasOptions( "A6,A7,A8" ).Choose( "A8" );
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

		var spirit = SpiritIs_FacturedDaysSplitTheSky();

		Give_SpiritOnA8(spirit);

		// Given: a spaces is put into stasis
		await spirit.When_ResolvingCard<AbsoluteStasis>( (user) => { 
			user.NextDecision.HasOptions("A1,A4,A5,A6,A7,A8").Choose("A5"); 
		} );

		//  When: targetting a second card
		await spirit.When_ResolvingCard<PillarOfLivingFlame>((user)=> {
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
		var spirit = SpiritIs_FacturedDaysSplitTheSky();

		Give_SpiritOnA8(spirit);

		// Given: the only SS is put into stasis
		await Given_SpacePutInStasis(spirit, "A8" );

		//  When: targetting a second card
		await PowerCard.ForDecorated(PillarOfLivingFlame.ActAsync).ActivateAsync( spirit )
			//  Then: action completes - no source to target from
			.ShouldComplete(PillarOfLivingFlame.Name);
	}

	// No Presence / Sacred Site in Stasis space
	[Trait( "Space", "State" )]
	[Fact]
	public async Task PresenceInStasis() {

		var spirit = SpiritIs_FacturedDaysSplitTheSky();

		Give_SpiritOnA8(spirit);

		// When: the only SS is put into stasis
		await Given_SpacePutInStasis(spirit, "A8");

		//  Then: no Presence found in A8
		spirit.Presence.Lands.Count( x => x.Label == "A8" ).ShouldBe( 0 );
		//   And: no SS found
		spirit.Presence.SacredSites.Count( x => x.Label == "A8" ).ShouldBe( 0 );
	}

	// Invaders Don't Explore Into / out of
	[Trait( "Space", "State" )]
	[Trait( "Invaders", "Explore" )]
	[Fact]
	public async Task NotASourceOfInvadersExploring() {
		var spirit = SpiritIs_FacturedDaysSplitTheSky();
		var board = GameState.Current.Island.Boards[0];

		Give_SpiritOnA8(spirit);
		SpaceSpec destination = board[7];
		SpaceSpec source = board[8];

		// Given: Town on A8
		source.Given_HasTokens("1T@2");
		// And: source is in stasis
		await Given_SpacePutInStasis(spirit, source.Label );

		// When: Invaders Explore - destination
		await destination.When_CardExplores();

		//  Then: no explorers in Jungle spaces.
		destination.ScopeSpace.HumanOfTag(Human.Explorer).Length.ShouldBe( 0 );
	}

	// Invaders Don't Explore Into / out of
	[Trait( "Space", "State" )]
	[Trait( "Invaders", "Explore" )]
	[Fact]
	public async Task NotADestinationOfInvadersExploring() {
		var spirit = SpiritIs_FacturedDaysSplitTheSky();
		var board = GameState.Current.Island.Boards[0];

		Give_SpiritOnA8(spirit);
		var destination = board[7];
		var source = board[8];

		// Given: Town on A8
		source.Given_HasTokens("1T@2");
		// And: Dst is in stasis
		await Given_SpacePutInStasis(spirit, destination.Label );

		// When: Invaders Explore - destination
		await destination.When_CardExplores();

		//  Then: no explorers in Jungle spaces.
		destination.ScopeSpace.HumanOfTag( Human.Explorer ).Length.ShouldBe( 0 );
	}

	// Invaders Don't Build
	[Trait( "Space", "State" )]
	[Trait( "Invaders", "Build" )]
	[Fact]
	public async Task NoBuild() {
		var spirit = SpiritIs_FacturedDaysSplitTheSky();
		var board = GameState.Current.Island.Boards[0];

		Give_SpiritOnA8(spirit);
		var space = board[7];

		// Given: Explorer on space
		space.Given_HasTokens( "1E@1" );
		// And: space is in stasis
		await Given_SpacePutInStasis(spirit, space.Label );
		Assert_SpaceHasCountTokens( space, Human.Town, 0 );

		// When: Invaders Build
		await space.When_CardBuilds();

		//  Then: no explorers in Jungle spaces.
		Assert_SpaceHasCountTokens( space, Human.Town, 0 );
	}

	// M Invaders Don't Ravage
	[Trait( "Space", "State" )]
	[Trait( "Invaders", "Ravage" )]
	[Fact]
	public async Task NoRavage() {
		var spirit = SpiritIs_FacturedDaysSplitTheSky();
		var board = GameState.Current.Island.Boards[0];

		Give_SpiritOnA8(spirit);
		var space = board[7];

		// Given: 1 Explorer and 1 Dahan on space
		space.Given_HasTokens( "1E@1,1D@2" );
		// And: space is in stasis
		await Given_SpacePutInStasis(spirit, space.Label );
		Assert_SpaceHasCountTokens( space, Human.Town, 0 );

		// When: Invaders Ravage
		await space.When_CardRavages();

		//  Then: explorers and dahan unchanged
		Assert_SpaceHasCountTokens( space, Human.Explorer, 1 );
	}

	// stasis space is included in win/loss check
	[Trait( "Space", "State" )]
	[Trait( "Feature", "Win/Loss")]
	[Fact]
	public async Task RecognizesPresenceForWinLoss() {
		var gs = new SoloGameState(new FracturedDaysSplitTheSky(), Boards.A);
		var spirit = gs.Spirit;
		var board = gs.Board;
		var space = board[7];
		var destroyPresenceSpace = board[3];


		// Given: Sacred Site on a space (so we can use Absolute Stasis Card)
		spirit.Given_IsOn(space, 2);
		//   And: presence somewhere else we can destory
		spirit.Given_IsOn(destroyPresenceSpace, 1);
		//   And: 1 city (so we don't win with 0 invaders)
		space.Given_HasTokens("1C@3");
		//   And: space is in stasis
		await Given_SpacePutInStasis( spirit, space.Label );
		//   And: should check win/loss
		gs.AddStandardWinLossChecks();

		try{
			//  When: destroy that extra presence (triggers win/loss check)
			await destroyPresenceSpace.ScopeSpace.Destroy(spirit.Presence.Token,1).ShouldComplete("destroying presence");
			//  When: we check win/loss
			gs.CheckWinLoss();
		} catch( GameOverException ) {
			// Then: no game-over exception is thrown and we get here
			throw new ShouldAssertException( "CheckWinLoss should not have thrown GameOverException." );
		}

	}

	[Trait( "Space", "State" )]
	[Fact]
	public async Task StillVisibleInAllSpaces() {
		var spirit = SpiritIs_FacturedDaysSplitTheSky();
		var board = GameState.Current.Island.Boards[0];

		Give_SpiritOnA8(spirit);
		var space = board[7];

		// When: space is put in stasis
		await Given_SpacePutInStasis( spirit, space.Label );

		// Then: space still apears in list of All Spaces
		ActionScope.Current.Spaces_Unfiltered.ShouldContain( space.ScopeSpace );
	}

	// Additional possible tests:

	// E Cannot Pull/Move From nor Push/Cascade/Move Into Stasis Space
	// Remove all calls to Space.Adjacent
	// Remove all calls to Space.Range(.)

	// H cannot destroy presence in Stasis space
	// Grinning Trickster removing blight
	// Thunderspeaker when Dahan destroyed

	// when round is over, space and tokens are restored

	Task Given_SpacePutInStasis(Spirit spirit, string spaceToStasisize ) {
		return spirit.When_ResolvingCard<AbsoluteStasis>( (user) => {
			user.NextDecision.HasOptions( "A1,A4,A5,A6,A7,A8" ).Choose( spaceToStasisize );
		} );
	}

	Spirit SpiritIs_FacturedDaysSplitTheSky() {
		var gs = new SoloGameState(new FracturedDaysSplitTheSky(), Boards.A);
		gs.InitMinorDeck();
		gs.InitMajorDeck();
		gs.Initialize();
		return gs.Spirit;
	}

	static void Give_SpiritOnA8(Spirit spirit) {
		spirit.Presence.SacredSites.Select(x => x.Label).Join(",").ShouldBe("A8");
	}

	void Assert_SpaceHasCountTokens( SpaceSpec space, ITokenClass tokenClass, int expectedCount ) {
		space.ScopeSpace.OfTag( tokenClass ).Length.ShouldBe( expectedCount );
	}

}
