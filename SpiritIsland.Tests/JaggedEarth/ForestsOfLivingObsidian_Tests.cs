﻿using SpiritIsland.JaggedEarth;

namespace SpiritIsland.Tests.JaggedEarth;

public class ForestsOfLivingObsidian_Tests {

	[Fact]
	public async Task Repeat_BadlandsWorksOnBothTargets() {
		var fix = new ConfigurableTestFixture();
		var space1 = fix.Board[3];
		var space2 = fix.Board[8];

		// Given: 2 sun, 3 fire, 3 earth
		fix.InitElements("2 sun,3 fire,3 earth");

		//  And: target land #1 has 1 presence, 3 explorers and 1 town
		fix.InitPresence(space1,1);
		fix.InitTokens(space1, "3E@1,1T@2");

		//  And: target land #2 has the same (1 presence, 3 explorers and 1 town)
		fix.InitPresence( space2, 1 );
		fix.InitTokens( space2, "3E@1,1T@2" );

		// When: play card
		var task = PowerCard.For<ForestsOfLivingObsidian>().ActivateAsync(fix.SelfCtx);
		//  And: targeting space 1
		fix.Choose(space1);
		fix.Choose("T@1"); // Damage (1 remaining)

		//  And: targeting space 2
		fix.Choose( space2 );
		fix.Choose( "T@1" ); // Damage (1 remaining)

		task.IsCompletedSuccessfully.ShouldBeTrue();
	}

	[Fact]
	public async Task Repeat_BadlandsWorksOnSameTargetTwice() {
		var fix = new ConfigurableTestFixture();
		var space1 = fix.Board[3];

		// Given: 2 sun, 3 fire, 3 earth
		fix.InitElements( "2 sun,3 fire,3 earth" );

		//  And: target land has 1 presence, 2 Cities
		fix.Spirit.Presence.Adjust( space1, 1 );
		fix.InitTokens( space1, "3C@3" );

		// When: play card
		var task = PowerCard.For<ForestsOfLivingObsidian>().ActivateAsync( fix.SelfCtx );
		//  And: targeting space 1
		fix.Choose( space1 );
		fix.Choose( "C@2" ); // 3C@2 - Damage 1 of them (1 of 1)
		fix.GameState.Tokens[space1].Summary.ShouldBe("1C@1,2C@2,1M");

		//  And: targeting space 1 a 2nd time
		fix.Choose( space1 );// Should Kill the 1C@1, and reduce the 2C@2 to 2C@1
		fix.Choose( "C@1" ); // Kill 1st City
		fix.Choose( "C@1" ); // Kill 2nd City
		fix.GameState.Tokens[space1].Summary.ShouldBe( "2M" );

		task.IsCompletedSuccessfully.ShouldBeTrue();
	}

}