using SpiritIsland.SinglePlayer;

namespace SpiritIsland.Tests.Spirits.River;

[Collection("BaseGame Spirits")]
public class MassiveFlooding_Tests {

	public MassiveFlooding_Tests():base(){}

	// Not enought elements -> nothing
	[Fact]
	public void InsufficientElements() {
		var spirit = new RiverSurges();
		var gs = new GameState( spirit, Board.BuildBoardA() ) { Phase = Phase.Slow };
		_ = new SinglePlayerGame(gs).StartAsync();


		var fixture = new ConfigurableTestFixture();
		var innatePower = MassiveFloodingPower;

		// Given: spirit does not have enough elements to trigger anything
		//   And: should not be activatable
		innatePower.CouldActivateDuring( Phase.Slow, fixture.Spirit ).ShouldBeFalse();

		//  When: but if we try anyway
		var task = innatePower.ActivateAsync( fixture.Spirit );

		//  Then: it is complete and nothing happens.
		task.IsCompleted.ShouldBeTrue();
	}

	[Trait("Feature","Push")]
	[Fact]
	public async Task Level1_Pushes1TownOrExplorer() { // 1-Sun, 2-Water
		RiverSurges spirit = new RiverSurges();
		Board board = Board.BuildBoardA();
		GameState gs = new GameState( spirit, board ) { Phase = Phase.Slow };
		Space space = board[5];
		SpaceState spaceState = gs.Tokens[space];
		SpaceState destination = spaceState.Adjacent.Last();

		// Given: spirit has a sacred site adjacent to the target space (range-1)
		SpiritExtensions.Given_Setup( spirit.Presence, spaceState.Adjacent.First(), 2 );
		//   And: Spirit has enough elements to trigger Level-1 of Massive Flooding
		spirit.Elements.Add( ElementStrings.Parse("1 sun,2 water" ) );
		//   And: target has 1 city, 4 towns, 5 explorers
		space.Tokens.Given_HasTokens( "1C@3,5E@1,4T@2" );

		//  When: activate innate
		await MassiveFloodingPower.ActivateAsync( spirit ).AwaitUser( spirit, user => { 
			user.NextDecision.Choose( space );
			user.NextDecision.MoveFrom("T@2").MoveTo(destination.Space.Text);
		} ).ShouldComplete();

		// Then: target has remaining invaders
		spaceState.Summary.ShouldBe( "1C@3,5E@1,3T@2" );

		//  And: destination had pushed invaders
		destination.Summary.ShouldBe( "1T@2" );

	}

	static InnatePower MassiveFloodingPower => InnatePower.For(typeof(MassiveFlooding));

	[Trait("Feature","Push")]
	[Fact]
	public async Task Level2_2DamagePush3TownOrExplorers() { // 2-Sun, 3-Water

		Spirit spirit = new RiverSurges();
		Board board = Boards.A;
		var gs = new GameState(spirit,board);
		
		// var fixture = new ConfigurableTestFixture();
		var space = board[5];
		var spaceState = gs.Tokens[space];
		var destination = space.Adjacent_Existing.Last();

		// Given: spirit has a sacred site adjacent to the target space (range-1)
		SpiritExtensions.Given_Setup( spirit.Presence, spaceState.Adjacent.First(), 2 );
		//   And: Spirit has enough elements to trigger Level-2 of Massive Flooding
		spirit.Elements.Add( ElementStrings.Parse("3 water,2 sun") );
		//   And: target has 1 city, 4 towns, 5 explorers
		var tokens = space.Tokens;
		tokens.InitDefault(Human.City,1);
		tokens.InitDefault(Human.Town,4);
		tokens.InitDefault(Human.Explorer,5);
		// fixture.InitTokens( space, "1C@3,4T@2,5E@1");

		//  When: activate innate
		await MassiveFloodingPower.ActivateAsync( spirit ).AwaitUser( spirit, user => {
			user.NextDecision.HasPrompt("Massive Flooding: Target Space").Choose( space ); // target space
			user.NextDecision.HasPrompt("Damage (2 remaining)").Choose( "T@2" ); // 1st damage
			user.NextDecision.HasPrompt("Damage (1 remaining)").Choose( "T@1" ); // 2nd damage

			user.NextDecision.HasPrompt("Push up to (3)").MoveFrom( "T@2", "T@2,E@1,Done" ).MoveTo( destination.Text );
			user.NextDecision.HasPrompt("Push up to (2)").MoveFrom( "T@2", "T@2,E@1,Done" ).MoveTo( destination.Text );
			user.NextDecision.HasPrompt("Push up to (1)").MoveFrom( "E@1", "T@2,E@1,Done" ).MoveTo( destination.Text );
		} ).ShouldComplete();

		// Then: target has remaining invaders
		spaceState.Summary.ShouldBe( "1C@3,4E@1,1T@2" );

		//  And: destination had pushed invaders
		gs.Tokens[destination].Summary.ShouldBe( "1E@1,2T@2" );
	}

	[Fact]
	public void Level3_2DamageToEachInvader() { // 3 sun, 4 water, 1 earth
		var spirit = new RiverSurges();
		var gs = new GameState( spirit, Board.BuildBoardA() ) { Phase = Phase.Slow };
		_ = new SinglePlayerGame(gs).StartAsync();



		var fixture = new ConfigurableTestFixture();
		var space = fixture.Board[5];
		var spaceState = fixture.GameState.Tokens[space];

		// Given: spirit has a sacred site adjacent to the target space (range-1)
		SpiritExtensions.Given_Setup( fixture.Spirit.Presence, spaceState.Adjacent.First(), 2 );
		//   And: Spirit has enough elements to trigger Level-3 of Massive Flooding
		fixture.InitElements( "3 sun,4 water,1 earth" );
		//   And: target has 1 city, 4 towns, 5 explorers
		fixture.InitTokens( space, "1C@3,4T@2,5E@1" );

		//  When: activate innate
		_ = MassiveFloodingPower.ActivateAsync( fixture.Spirit );
		fixture.Choose( space ); // target space

		// Then: target has remaining invaders
		spaceState.Summary.ShouldBe( "1C@1" );
	}

}