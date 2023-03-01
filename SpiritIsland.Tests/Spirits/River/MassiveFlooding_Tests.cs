using SpiritIsland.SinglePlayer;

namespace SpiritIsland.Tests.Spirits.River;

public class MassiveFlooding_Tests : RiverGame {

	readonly GameState gs;

	public MassiveFlooding_Tests():base(){
		ActionScope.Initialize();

		// Given: River
		gs = new GameState( spirit, Board.BuildBoardA() ) {
			Phase = Phase.Slow
		};

		_game = new SinglePlayerGame(gs).Start();

	}

	// Not enought elements -> nothing
	[Fact]
	public void InsufficientElements() {
		var fixture = new ConfigurableTestFixture();
		var innatePower = InnatePower.For<MassiveFlooding>();

		// Given: spirit does not have enough elements to trigger anything
		//   And: should not be activatable
		innatePower.CouldActivateDuring( Phase.Slow, fixture.Spirit ).ShouldBeFalse();

		//  When: but if we try anyway
		var task = innatePower.ActivateAsync( fixture.SelfCtx );

		//  Then: it is complete and nothing happens.
		task.IsCompleted.ShouldBeTrue();
	}

	[Trait("Feature","Push")]
	[Fact]
	public void Level1_Pushes1TownOrExplorer() { // 1-Sun, 2-Water
		var fixture = new ConfigurableTestFixture();
		var space = fixture.Board[5];
		var spaceState = fixture.GameState.Tokens[space];
		var destination = spaceState.Adjacent.Last();

		// Given: spirit has a sacred site adjacent to the target space (range-1)
		SpiritExtensions.Adjust( fixture.Spirit.Presence, spaceState.Adjacent.First(), 2 );
		//   And: Spirit has enough elements to trigger Level-1 of Massive Flooding
		fixture.InitElements( "1 sun,2 water" );
		//   And: target has 1 city, 4 towns, 5 explorers
		fixture.InitTokens( space, "1C@3,5E@1,4T@2" );

		//  When: activate innate
		_ = InnatePower.For<MassiveFlooding>().ActivateAsync( fixture.SelfCtx );
		fixture.Choose( space ); // target space

		fixture.ChoosePush( StdTokens.Town, destination.Space ); // push 1

		// Then: target has remaining invaders
		spaceState.Summary.ShouldBe( "1C@3,5E@1,3T@2" );

		//  And: destination had pushed invaders
		destination.Summary.ShouldBe( "1T@2" );

	}

	[Trait("Feature","Push")]
	[Fact]
	public void Level2_2DamagePush3TownOrExplorers() { // 2-Sun, 3-Water

		var fixture = new ConfigurableTestFixture();
		var space = fixture.Board[5];
		var spaceState = fixture.GameState.Tokens[space];
		var destination = space.Adjacent_Existing.Last();

		// Given: spirit has a sacred site adjacent to the target space (range-1)
		SpiritExtensions.Adjust( fixture.Spirit.Presence, spaceState.Adjacent.First(), 2 );
		//   And: Spirit has enough elements to trigger Level-2 of Massive Flooding
		fixture.InitElements("3 water,2 sun");
		//   And: target has 1 city, 4 towns, 5 explorers
		fixture.InitTokens( space, "1C@3,4T@2,5E@1");

		//  When: activate innate
		_ = InnatePower.For<MassiveFlooding>().ActivateAsync( fixture.SelfCtx );
		fixture.Choose( space ); // target space
		fixture.Choose( StdTokens.Town ); // 1st damage
		fixture.Choose( StdTokens.Town1 ); // 2nd damage

		fixture.ChoosePush( StdTokens.Town, destination ); // push 1
		fixture.ChoosePush( StdTokens.Town, destination ); // push 2
		fixture.ChoosePush( StdTokens.Explorer, destination ); // push 3

		// Then: target has remaining invaders
		spaceState.Summary.ShouldBe( "1C@3,4E@1,1T@2" );

		//  And: destination had pushed invaders
		fixture.GameState.Tokens[destination].Summary.ShouldBe( "1E@1,2T@2" );
	}

	[Fact]
	public void Level3_2DamageToEachInvader() { // 3 sun, 4 water, 1 earth

		var fixture = new ConfigurableTestFixture();
		var space = fixture.Board[5];
		var spaceState = fixture.GameState.Tokens[space];

		// Given: spirit has a sacred site adjacent to the target space (range-1)
		SpiritExtensions.Adjust( fixture.Spirit.Presence, spaceState.Adjacent.First(), 2 );
		//   And: Spirit has enough elements to trigger Level-3 of Massive Flooding
		fixture.InitElements( "3 sun,4 water,1 earth" );
		//   And: target has 1 city, 4 towns, 5 explorers
		fixture.InitTokens( space, "1C@3,4T@2,5E@1" );

		//  When: activate innate
		_ = InnatePower.For<MassiveFlooding>().ActivateAsync( fixture.SelfCtx );
		fixture.Choose( space ); // target space

		// Then: target has remaining invaders
		spaceState.Summary.ShouldBe( "1C@1" );
	}

}