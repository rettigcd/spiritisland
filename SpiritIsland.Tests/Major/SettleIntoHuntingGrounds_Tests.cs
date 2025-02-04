namespace SpiritIsland.Tests.Major;

public class SettleIntoHuntingGrounds_Tests {

	// Other things we could test:
	// - Gather Presence / SS of another spirit

	[Fact]
	public async Task CanFreezePresence() {
		var (spirit,board) = Init();
		Space a5 = board[5].ScopeSpace;

		// Given: Presence on a5 and a6
		a5.Given_HasTokens("1RSiS");

		await spirit.When_ResolvingCard<SettleIntoHuntingGrounds>();

		a5.Summary.ShouldBe("1A-😀,1M-😀,1RSiS");
		a5.Sum(Token.Beast).ShouldBe(1);
		a5.Sum(Token.Badlands).ShouldBe(1);
	}

	[Trait("SpecialRule","AllyOfTheDahan")]
	[Fact]
	public async Task Disables_AllyOfTheDahan() {
		Spirit spirit = new Thunderspeaker();
		Board board = Boards.A;
		SpaceSpec a1 = board[1];
		GameState gameState = new SoloGameState( spirit, board );

		// Given: presence & Dahan on A1 
		a1.Given_HasTokens("1Ts,1D@2");

		//   And: Settled Into Hunting Grounds
		await spirit.When_ResolvingCard<SettleIntoHuntingGrounds>();

		//  When: pushing dahan
		await CallToMigrate.ActAsync( spirit.Target(a1) ).AwaitUser( u => {
			u.NextDecision.HasPrompt("Push up to (1)").ChooseFrom("D@2").ChooseTo("A2", "A2,A4,A5,A6");
			u.NextDecision.HasPrompt("Move presence with Dahan?")
				.HasOptions("Ts on A1,Done")
				.Choose("Ts on A1");
			// Then: action completes without Thunderspeaker riding along.
		}).ShouldComplete();

		//  Then: TS is still on A1
		spirit.Presence.IsOn(a1.ScopeSpace).ShouldBeTrue();

		//   And: not on A2
		spirit.Presence.IsOn(board[2].ScopeSpace).ShouldBeFalse();
	}

	[Trait( "SpecialRule", "Mists Shift and Flow" )]
	[Theory]
	[InlineData( false, "A1,A2,A4,A5,A6", "A2" )]
	// [InlineData( true, "A1", "A1" )]    This is how we want to behave, but it will present all of the non-selectable options, not sure what happens if user selects non-selectable option.
	public async Task DisablesMistsShiftAndFlow( bool playSettleIntoHuntingGround, string expectedTargetOptions, string pick ) {
		Spirit spirit = new ShroudOfSilentMist();
		Board board = Boards.A;
		GameState gameState = new SoloGameState( spirit, board );

		// Given: presence in A1
		board[1].Given_HasTokens( "1SoSM" );

		// Given: Settled Into Hunting Grounds
		if(playSettleIntoHuntingGround)
			await spirit.When_ResolvingCard<SettleIntoHuntingGrounds>();

		//  When: targetting range-0
		await spirit.When_ResolvingCard<MesmerizedTranquility>( u => {
			u.NextDecision.HasPrompt( "Mesmerized Tranquility: Target Space" ).HasOptions( expectedTargetOptions ).Choose( pick );
		} );
	}

	[Fact]
	public async Task ProtectsFromDrawingIntoTheVoid() {
		Spirit spirit = new RiverSurges();
		Board board = Boards.A;
		GameState gameState = new SoloGameState( spirit, board );
		Space a1 = board[1].ScopeSpace;
		Space a2 = board[2].ScopeSpace;

		// Given: spirit on A1 and A2
		a1.Given_HasTokens("1RSiS");
		a2.Given_HasTokens("1RSiS");

		//   And: played Settle into hunting grounds
		await spirit.When_ResolvingCard<SettleIntoHuntingGrounds>();

		// When: playing Draw into ever consuming void
		await DrawTowardsAConsumingVoid.ActAsync( spirit.Target(a2) ).AwaitUser( u => {
			// Then: we still have to select presence
			u.NextDecision.HasPrompt("Select presence to move.").HasOptions("RSiS on A1").Choose("RSiS on A1");
		}).ShouldComplete();

		// But: spirit is still on A1 (doesn't move)
		a1.Summary.ShouldBe("1A-😀,1M-😀,1RSiS");
	}

	[Fact]
	public async Task BeastIsFoundButDoesntMove() {
		var gs = new SoloGameState();
		var originalLand = gs.Board[7].ScopeSpace;

		// Given: presence settles into a land 
		gs.Spirit.Given_IsOn(originalLand);
		await SettleIntoHuntingGrounds.ActAsync(gs.Spirit);
		//   And: a real beast
		originalLand.Init(Token.Beast,1);

		// When: Softly beckoning from an adjacent land
		var beckonedTo = gs.Board[8];
		await SoftlyBeckonEverInward.ActAsync(gs.Spirit.Target(beckonedTo)).AwaitUser(user => {
			// Can gather the regular beast
			user.NextDecision.HasPrompt("Gather up to (2)").HasOptions("Beast-😀 on A7 => A8,Beast on A7 => A8,Done").Choose("Beast on A7 => A8");
			// And can see the special beast and select it
			user.NextDecision.HasPrompt("Gather up to (1)").HasOptions("Beast-😀 on A7 => A8,Done").Choose("Beast-😀 on A7 => A8");
		}).ShouldComplete("Softly Beckon");

		// But only the regular beast moved
		beckonedTo.ScopeSpace.Summary.ShouldBe("1A");
		originalLand.Summary.ShouldBe("1A-😀,1M-😀,1TS");
	}

	[Fact]
	public async Task BadlandDealsDamageDuringRavage() {
		// pre-pressumes Spirit wants presence to be badlands here.

		var gs = new SoloGameState();
		var land = gs.Board[7].ScopeSpace;

		// Given: presence settles into a land 
		gs.Spirit.Given_IsOn(land);
		await SettleIntoHuntingGrounds.ActAsync(gs.Spirit);

		//   And: space has 1 Dahan and 1 explorer
		land.Given_HasTokens("1D@2,1E@1");

		//  When: ravaging in that land
		await land.When_CardRavages();

		//  Then: explorer kills dahan
		land.Summary.ShouldBe("1A-😀,1E@1,1M-😀,1TS");
	}

	[Fact]
	public async Task BadlandDealsDamageForSpiritPower() {
		// pre-pressumes Spirit wants presence to be badlands here.

		var gs = new SoloGameState();
		var land = gs.Board[7].ScopeSpace;

		// Given: presence settles into a land 
		gs.Spirit.Given_IsOn(land);
		await SettleIntoHuntingGrounds.ActAsync(gs.Spirit);

		//   And: space has 1 City
		land.InitDefault(Human.City,1);

		//  When: ravaging in that land
		await SwallowedByTheWilderness.ActAsync(gs.Spirit.Target(land)).AwaitUser(user => {
			// 3 points of damage (1 Beast + 1 Badlands) = 2 initial points of damage +1 badlands effect boosts to 3
			user.NextDecision.HasPrompt("Damage (3 remaining)").HasOptions("C@3 on A7").ChooseFirst();
			user.NextDecision.HasPrompt("Damage (2 remaining)").HasOptions("C@2 on A7").ChooseFirst();
			user.NextDecision.HasPrompt("Damage (1 remaining)").HasOptions("C@1 on A7").ChooseFirst();
		}).ShouldComplete("Swallow by the wilderness");

		//  Then: explorer kills dahan
		land.Summary.ShouldBe("1A-😀,1M-😀,1TS");
	}


	[Fact]
	public async Task CantTakeFromBoard() {
		Spirit spirit = new RiverSurges();
		Board board = Boards.A;
		GameState gameState = new SoloGameState( spirit, board );
		SpaceSpec a1 = board[1];
	
		// Given: spirit on A1
		a1.Given_HasTokens( "2RSiS" );

		//   And: played Settle into hunting grounds
		await spirit.When_ResolvingCard<SettleIntoHuntingGrounds>();

		// When: playing Unrelenting Growth
		await spirit.When_ResolvingCard<UnrelentingGrowth>( u => {
			u.NextDecision.HasPrompt( "Where would you like to place your presence?" ).Choose("A5");
			// Then: Presence still appears and is selectable
			u.NextDecision.HasPrompt( "Select Presence to place" ).HasOptions( "2 energy,2 cardplay,RSiS" ).Choose( "RSiS" );
			u.NextDecision.HasPrompt( "Select Presence to place" ).HasOptions( "2 energy,2 cardplay,RSiS" ).Choose( "RSiS" ); 
		} );

		// But: is not move placed
		spirit.Presence.CountOn(a1.ScopeSpace).ShouldBe(2);
		spirit.Presence.CountOn(board[5].ScopeSpace).ShouldBe(0);

	}

	//[Facts]
	//public async Task 

	static (Spirit,Board) Init() {
		RiverSurges spirit = new RiverSurges();
		Board board = Boards.A;
		var gs = new SoloGameState(spirit,board);
		return (spirit,board);
	}

}
