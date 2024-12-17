namespace SpiritIsland.Tests.Major;

public class ForestsOfLivingObsidian_Tests {

	[Trait( "Token", "Badlands" )]
	[Trait( "Feature", "Repeat" )]
	[Fact]
	public Task Repeat_BadlandsWorksOnBothTargets() {
		// var fix = new ConfigurableTestFixture();

		// Given: spirit with no SS
		var gs = new SoloGameState(new LightningsSwiftStrike());

		var a3 = gs.Board[3];
		var a8 = gs.Board[8];

		// Given: 2 sun, 3 fire, 3 earth
		gs.Spirit.Configure().Elements("2 sun,3 fire,3 earth");

		//  And: target land #1 has 1 presence, 3 explorers and 1 town
		gs.Spirit.Given_IsOn(a3);
		a3.ScopeSpace.Given_HasTokens("3E@1,1T@2");
		// fix.InitTokens(space1, "3E@1,1T@2");

		//  And: target land #2 has the same (1 presence, 3 explorers and 1 town)
		gs.Spirit.Given_IsOn(a8);
		a8.ScopeSpace.Given_HasTokens("3E@1,1T@2");
		//fix.InitPresence( space2, 1 );
		//fix.InitTokens( space2, "3E@1,1T@2" );

		// When: play card
		return gs.Spirit.When_ResolvingCard<ForestsOfLivingObsidian>(user => {

			//  And: targeting space 1
			user.NextDecision.HasPrompt("Forests of Living Obsidian: Target Space").HasOptions("A3,A8").Choose("A3");

			// And: Does 1 damage to each invader (because there is no SS)

			// Then: badlands asks user to select recipient of remaining badlans.
			user.Choose("T@1"); // Damage (1 remaining)

			//  And: Same on target 2
			user.NextDecision.HasPrompt("Forests of Living Obsidian: Target Space").HasOptions("A3,A8").Choose("A8");
			user.Choose("T@1"); // Damage (1 remaining)
		});

		//return PowerCard.For(typeof(ForestsOfLivingObsidian)).ActivateAsync(fix.Spirit).AwaitUser(u => {
		//	u.NextDecision.HasPrompt("Forests of Living Obsidian: Target Space").HasOptions("A3,A8").Choose("A3");
		//	u.StubChoice();
		//	//  And: targeting space 1
		//	u.Choose(space1);
		//	u.Choose("T@1"); // Damage (1 remaining)
		//	//  And: targeting space 2
		//	u.Choose(space2);
		//	u.Choose("T@1"); // Damage (1 remaining)
		//}).ShouldComplete();
	}

	[Trait( "Token", "Badlands" )]
	[Trait( "Feature", "Repeat" )]
	[Fact]
	public async Task Repeat_BadlandsWorksOnSameTargetTwice() {
		var fix = new ConfigurableTestFixture();
		var space1 = fix.Board[3];
		var space = fix.GameState.Tokens[space1];

		// Given: 2 sun, 3 fire, 3 earth
		fix.InitElements( "2 sun,3 fire,3 earth" );

		//  And: target land has 1 presence, 2 Cities
		fix.Spirit.Given_IsOn( space );
		fix.InitTokens( space1, "3C@3" );

		// When: play card
		var task = PowerCard.ForDecorated(ForestsOfLivingObsidian.ActAsync).ActivateAsync( fix.Spirit );
		//  And: targeting space 1
		fix.Choose( space1 );
		fix.Choose( "C@2" ); // 3C@2 - Damage 1 of them (1 of 1)

		//  And: targeting space 1 a 2nd time
		fix.Choose( space1 );// Should Kill the 1C@1, and reduce the 2C@2 to 2C@1
		fix.Choose( "C@1" ); // Kill 1st City
		fix.Choose( "C@1" ); // Kill 2nd City
		await task.ShouldComplete();
		space.Summary.ShouldBe( "2M,1RSiS" );

		task.IsCompletedSuccessfully.ShouldBeTrue();
	}

}