namespace SpiritIsland.Tests.Major;

public class SettleIntoHuntingGrounds_Tests {

	// Other things we could test:
	// - Gather Presence / SS of another spirit

	[Fact]
	public async Task CanFreezePresence() {
		var (spirit,board) = Init();
		SpaceState a5 = board[5];

		// Given: Presence on a5 and a6
		a5.Given_HasTokens("1RSiS");

		await spirit.When_ResolvingCard<SettleIntoHuntingGrounds>();
	}

	[Trait("SpecialRule","AllyOfTheDahan")]
	[Fact]
	public async Task Disables_AllyOfTheDahan() {
		Spirit spirit = new Thunderspeaker();
		Board board = Board.BuildBoardA();
		Space a1 = board[1];
		GameState gameState = new GameState( spirit, board );

		// Given: presence & Dahan on A1 
		a1.Given_HasTokens("1Ts,1D@2");

		//   And: Settled Into Hunting Grounds
		await spirit.When_ResolvingCard<SettleIntoHuntingGrounds>();

		//  When: pushing dahan
		await spirit.When_TargetingSpace( a1, CallToMigrate.ActAsync, u => {
			u.NextDecision.HasPrompt( "Push up to (1)" ).MoveFrom( "D@2" ).MoveTo("A2","A2,A4,A5,A6");

			u.NextDecision.HasPrompt( "Move presence with Dahan?" )
				.HasOptions("Ts on A1,Done")
				.Choose( "Ts on A1" );

			//  Then: action completes without Thunderspeaker riding along.
		} );

		//  Then: TS is still on A1
		spirit.Presence.IsOn(a1.Tokens).ShouldBeTrue();

		//   And: not on A2
		spirit.Presence.IsOn(board[2].Tokens).ShouldBeFalse();
	}

	[Trait( "SpecialRule", "Mists Shift and Flow" )]
	[Theory]
	[InlineData( false, "A1,A2,A4,A5,A6", "A2" )]
	// [InlineData( true, "A1", "A1" )]    This is how we want to behave, but it will present all of the non-selectable options, not sure what happens if user selects non-selectable option.
	public async Task DisablesMistsShiftAndFlow( bool playSettleIntoHuntingGround, string expectedTargetOptions, string pick ) {
		Spirit spirit = new ShroudOfSilentMist();
		Board board = Board.BuildBoardA();
		GameState gameState = new GameState( spirit, board );

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
		Board board = Board.BuildBoardA();
		GameState gameState = new GameState( spirit, board );
		Space a1 = board[1];
		Space a2 = board[2];

		// Given: spirit on A1 and A2
		a1.Given_HasTokens("1RSiS");
		a2.Given_HasTokens("1RSiS");

		//   And: played Settle into hunting grounds
		await spirit.When_ResolvingCard<SettleIntoHuntingGrounds>();

		// When: playing Draw into ever consuming void
		await spirit.When_TargetingSpace( a2, DrawTowardsAConsumingVoid.ActAsync, u => {
			// Then: we still have to select presence
			u.NextDecision.HasPrompt("Select presence to move.").HasOptions("RSiS on A1").Choose("RSiS on A1");
		} );
		// But: spirit is still on A1 (doesn't move)
		a1.Tokens.Summary.ShouldBe("1RSiS");
	}

	[Fact]
	public async Task CantTakeFromBoard() {
		Spirit spirit = new RiverSurges();
		Board board = Board.BuildBoardA();
		GameState gameState = new GameState( spirit, board );
		Space a1 = board[1];
	
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
		spirit.Presence.CountOn(a1.Tokens).ShouldBe(2);
		spirit.Presence.CountOn(board[5].Tokens).ShouldBe(0);



	}

	static (Spirit,Board) Init() {
		RiverSurges spirit = new RiverSurges();
		Board board = Board.BuildBoardA();
		_ = new GameState(spirit,board);
		return (spirit,board);
	}

}
