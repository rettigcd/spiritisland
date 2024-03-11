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
		new SinglePlayerGame(gs).Start();


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
		SpaceSpec spaceSpec = board[5];
		Space space = gs.Tokens[spaceSpec];
		Space destination = space.Adjacent.Last();

		// Given: spirit has a sacred site adjacent to the target space (range-1)
		spirit.Given_IsOn( space.Adjacent.First(), 2 );
		//   And: Spirit has enough elements to trigger Level-1 of Massive Flooding
		spirit.Elements.Add( ElementStrings.Parse("1 sun,2 water" ) );
		//   And: target has 1 city, 4 towns, 5 explorers
		spaceSpec.ScopeSpace.Given_HasTokens( "1C@3,5E@1,4T@2" );

		//  When: activate innate
		await MassiveFloodingPower.ActivateAsync( spirit ).AwaitUser( user => { 
			user.NextDecision.Choose( spaceSpec );
			user.NextDecision.MoveFrom("T@2").MoveTo(destination.Label);
		} ).ShouldComplete();

		// Then: target has remaining invaders
		space.Summary.ShouldBe( "1C@3,5E@1,3T@2" );

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
		var spaceSpec = board[5];
		var space = gs.Tokens[spaceSpec];
		var destination = spaceSpec.Adjacent_Existing.Last();

		// Given: spirit has a sacred site adjacent to the target space (range-1)
		spirit.Given_IsOn( space.Adjacent.First(), 2 );
		//   And: Spirit has enough elements to trigger Level-2 of Massive Flooding
		spirit.Elements.Add( ElementStrings.Parse("3 water,2 sun") );
		//   And: target has 1 city, 4 towns, 5 explorers
		var tokens = spaceSpec.ScopeSpace;
		tokens.InitDefault(Human.City,1);
		tokens.InitDefault(Human.Town,4);
		tokens.InitDefault(Human.Explorer,5);
		// fixture.InitTokens( space, "1C@3,4T@2,5E@1");

		//  When: activate innate
		await MassiveFloodingPower.ActivateAsync( spirit ).AwaitUser( user => {
			user.NextDecision.HasPrompt("Massive Flooding: Target Space").Choose( spaceSpec ); // target space
			user.NextDecision.HasPrompt("Damage (2 remaining)").Choose( "T@2" ); // 1st damage
			user.NextDecision.HasPrompt("Damage (1 remaining)").Choose( "T@1" ); // 2nd damage

			user.NextDecision.HasPrompt("Push up to (3)").MoveFrom( "T@2", "T@2,E@1,Done" ).MoveTo( destination.Label );
			user.NextDecision.HasPrompt("Push up to (2)").MoveFrom( "T@2", "T@2,E@1,Done" ).MoveTo( destination.Label );
			user.NextDecision.HasPrompt("Push up to (1)").MoveFrom( "E@1", "T@2,E@1,Done" ).MoveTo( destination.Label );
		} ).ShouldComplete();

		// Then: target has remaining invaders
		space.Summary.ShouldBe( "1C@3,4E@1,1T@2" );

		//  And: destination had pushed invaders
		gs.Tokens[destination].Summary.ShouldBe( "1E@1,2T@2" );
	}

	[Fact]
	public void Level3_2DamageToEachInvader() { // 3 sun, 4 water, 1 earth
		var spirit = new RiverSurges();
		var gs = new GameState( spirit, Board.BuildBoardA() ) { Phase = Phase.Slow };
		new SinglePlayerGame(gs).Start();



		var fixture = new ConfigurableTestFixture();
		var spaceSpec = fixture.Board[5];
		var space = fixture.GameState.Tokens[spaceSpec];

		// Given: spirit has a sacred site adjacent to the target space (range-1)
		fixture.Spirit.Given_IsOn( space.Adjacent.First(), 2 );
		//   And: Spirit has enough elements to trigger Level-3 of Massive Flooding
		fixture.InitElements( "3 sun,4 water,1 earth" );
		//   And: target has 1 city, 4 towns, 5 explorers
		fixture.InitTokens( spaceSpec, "1C@3,4T@2,5E@1" );

		//  When: activate innate
		_ = MassiveFloodingPower.ActivateAsync( fixture.Spirit );
		fixture.Choose( spaceSpec ); // target space

		// Then: target has remaining invaders
		space.Summary.ShouldBe( "1C@1" );
	}

}