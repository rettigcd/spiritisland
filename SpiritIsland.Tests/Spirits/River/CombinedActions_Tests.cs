namespace SpiritIsland.Tests.Spirits.River;

/// <summary>
/// Spirit.CombineActionsAndTargets: the user picks a power together with the land it will target (like a Move is a token together
/// with where it goes), instead of picking the power and then, in a second decision, its target. Off by default, so existing
/// callers are unchanged.
/// </summary>
[Collection("BaseGame Spirits")]
public class CombinedActions_Tests {

	readonly Spirit _spirit = new RiverSurges();
	GameState _gameState;
	PowerCard _washAway;

	/// <summary> River with Wash Away in play in the Slow phase, presence on A5, and an explorer on A4 to push. </summary>
	void Given_WashAwayIsReady( bool combine ) {
		_gameState = new SoloGameState( _spirit, Boards.A );
		Board board = Boards.A;
		_gameState.Island = new Island( board );
		_spirit.Given_IsOn( board.Spaces.Single( s => s.Label == "A5" ) );
		_washAway = _spirit.Hand.Single( c => c.Title == WashAway.Name );
		_spirit.Energy = _washAway.Cost;
		_spirit.PlayCard( _washAway );
		_spirit.Assert_CardIsReady( _washAway, Phase.Slow );
		_gameState.Phase = Phase.Slow;
		_gameState.Tokens[board[4]].AdjustDefault( Human.Explorer, 1 );
		_spirit.CombineActionsAndTargets = combine;
	}

	[Fact]
	public void IsOffByDefault() {
		_spirit.CombineActionsAndTargets.ShouldBeFalse();
	}

	[Fact]
	public async Task Off_ThePowerIsPickedFirst_ThenItsTarget() {
		Given_WashAwayIsReady( combine: false );

		await _spirit.SelectAndResolveNextAction( _gameState ).AwaitUser( u => {
			u.NextDecision.HasPrompt( "Select Slow to resolve" ).HasOptions( "Wash Away $1 (Slow),Done" ).Choose( "Wash Away $1 (Slow)" );
			u.NextDecision.HasTargetSpacePrompt( "Wash Away" ).Choose( "A4" );
			u.NextDecision.HasPrompt( "Push up to (1)" ).ChooseFrom( "E@1", "E@1,Done" ).ChooseTo( "A2", "A1,A2,A3,A5" );
		} ).ShouldComplete();

		Boards.A[2].Assert_HasInvaders( "1E@1" );
	}

	[Fact]
	public async Task On_TheMenuOffersThePowerWithEachLandItCanTarget() {
		Given_WashAwayIsReady( combine: true );

		string[] landsItCanTarget = [.. (await _washAway.GetTargetSpaceOptions( _spirit ))!.Select( s => s.SpaceSpec.Label ).Order()];
		landsItCanTarget.Length.ShouldBeGreaterThan( 1 );

		IOption[] menu = await _spirit.GetCombinedActionMenu( _spirit.GetActionMenu( Phase.Slow ) );

		menu.ShouldAllBe( o => o is TargetedAction );
		menu.Select( o => o.Text ).ShouldBe( landsItCanTarget.Select( l => $"Wash Away $1 (Slow) => {l}" ) );
	}

	[Fact]
	public async Task On_ThePowerAndItsTargetArePickedTogether_SoTheTargetIsNotAskedFor() {
		Given_WashAwayIsReady( combine: true );

		await _spirit.SelectAndResolveNextAction( _gameState ).AwaitUser( u => {
			u.NextDecision.HasPrompt( "Select Slow to resolve" ).Choose( "Wash Away $1 (Slow) => A4" );
			// no "Wash Away: Target Space" decision: straight to what the power does
			u.NextDecision.HasPrompt( "Push up to (1)" ).ChooseFrom( "E@1", "E@1,Done" ).ChooseTo( "A2", "A1,A2,A3,A5" );
		} ).ShouldComplete();

		Boards.A[4].Assert_HasInvaders( "" );
		Boards.A[2].Assert_HasInvaders( "1E@1" );
	}

	[Fact]
	public async Task On_DoneIsStillAnOption_AndEndsTheSelecting() {
		Given_WashAwayIsReady( combine: true );

		Task<bool> selecting = _spirit.SelectAndResolveNextAction( _gameState );
		await selecting.AwaitUser( u => u.NextDecision.HasPrompt( "Select Slow to resolve" ).Choose( "Done" ) ).ShouldComplete();

		( await selecting ).ShouldBeFalse();
	}

	[Fact]
	public async Task On_APowerThatTargetsASpirit_IsOfferedWithEachSpiritItCanTarget() {
		Given_WashAwayIsReady( combine: true );
		PowerCard solidify = PowerCard.For( typeof( SolidifyEchoesOfMajestyPast ) ); // [AnySpirit]

		IOption[] menu = await _spirit.GetCombinedActionMenu( [solidify] );

		// there is only one spirit, but the form is the same as for a land: drop the power on the spirit it targets
		menu.Select( o => o.Text ).ShouldBe( [$"Solidify Echoes of Majesty Past $4 (Fast) => {_spirit.SpiritName}"] );
		( (TargetedAction)menu[0] ).Target.ShouldBe( _spirit );
	}

	[Fact]
	public async Task On_CarapacedLand_IsOfferedWithItsOwnTwoRanges() {
		Given_WashAwayIsReady( combine: true ); // presence on A5, whose neighbours are A1, A2, A3 and A4 (and A6)
		Board board = Boards.A;
		_gameState.Tokens[board[4]].Init( Token.Beast, 1 ); // 1 away, with a beast: in range
		PowerCard carapaced = PowerCard.For( typeof( CarapacedLand ) );

		string[] offered = [.. ( await _spirit.GetCombinedActionMenu( [carapaced] ) ).Select( o => o.Text )];

		offered.ShouldBe( [
			"Carapaced Land $0 (Fast) => A4", // 1 away, but it has a beast
			"Carapaced Land $0 (Fast) => A5", // where the presence is
		] );
	}

	[Fact]
	public async Task On_GrowthActionsAreOfferedWithTheSpiritThatGrows() {
		Given_WashAwayIsReady( combine: true );
		_gameState.Phase = Phase.Growth;
		_spirit.GrowthTrack.Reset();

		IOption[] menu = await _spirit.GetCombinedActionMenu( _spirit.GetGrowthMenu() );

		menu.ShouldNotBeEmpty();
		menu.ShouldAllBe( o => o is TargetedAction && ( (TargetedAction)o ).Target == _spirit );
		menu.Select( o => o.Text ).ShouldContain( $"Gain 1 Energy => {_spirit.SpiritName}" );
	}

	[Fact]
	public async Task On_AGrowthActionAndItsSpiritArePickedTogether() {
		Given_WashAwayIsReady( combine: true );
		_gameState.Phase = Phase.Growth;
		_spirit.GrowthTrack.Reset();
		int energy = _spirit.Energy;

		await _spirit.SelectAndResolveNextGrowthAction().AwaitUser( u => {
			u.NextDecision.HasPrompt( "Select Growth" ).Choose( $"Gain 1 Energy => {_spirit.SpiritName}" );
		} ).ShouldComplete();

		_spirit.Energy.ShouldBe( energy + 1 );
	}

	[Fact]
	public async Task On_AChosenTargetDoesNotLeakIntoTheNextAction() {
		Given_WashAwayIsReady( combine: true );
		await _spirit.SelectAndResolveNextAction( _gameState ).AwaitUser( u => {
			u.NextDecision.HasPrompt( "Select Slow to resolve" ).Choose( "Wash Away $1 (Slow) => A4" );
			u.NextDecision.HasPrompt( "Push up to (1)" ).ChooseFrom( "E@1", "E@1,Done" ).ChooseTo( "A2", "A1,A2,A3,A5" );
		} ).ShouldComplete();

		// using the same power directly afterwards (without choosing a target with it) asks for its target as usual
		await _washAway.ActivateAsync( _spirit ).AwaitUser( u => {
			u.NextDecision.HasTargetSpacePrompt( "Wash Away" ).Choose( "A4" );
		} ).ShouldComplete();
	}

}
