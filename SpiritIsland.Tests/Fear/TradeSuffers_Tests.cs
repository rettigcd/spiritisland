namespace SpiritIsland.Tests.Fear;

public class TradeSuffers_Tests {

	[Fact]
	[Trait( "Invaders", "Build" )]
	public async Task Level1_CityPresent_NoBuild() {

		var gs = new SoloGameState(new RiverSurges(), Boards.B);
		var b5 = gs.Island.Boards[0][5].ScopeSpace;

		// Given: City on Space stops build
		b5.Given_InitSummary("1C@3");

		//  When: Trade Suffers - Level 1
		await new TradeSuffers().ActAsync(1);

		//   And: builds
		await b5.When_CardBuilds();

		//  Then: no town is built
		b5.Summary.ShouldBe("1C@3");

	}

	[Fact]
	[Trait("Invaders", "Build")]
	public async Task Level1_CityAddedAfterFear_NoBuild() {

		// Tests that city is evaluated at Build Time, not at Fear time.

		var gs = new SoloGameState(new RiverSurges(), Boards.B);
		var b5 = gs.Island.Boards[0][5].ScopeSpace;

		// Given: No City on Space
		b5.InitDefault(Human.City,0);

		//   And: Trade Suffers - Level 1
		await new TradeSuffers().ActAsync(1);

		//  When: city is added after the fact.  (Like from a Blighted island during Ravage)
		b5.InitDefault(Human.City, 1);

		//   And: builds
		await b5.When_CardBuilds();

		//  Then: no town is built
		b5.Summary.ShouldBe("1C@3");

	}

	[Fact]
	public async Task Level1_CityDestroyedDuringRavage_Build() {

		var gs = new SoloGameState();
		var space = gs.Board[7].ScopeSpace;

		// Given: Explorer and City on space.
		space.InitDefault(Human.City,1);
		space.InitDefault(Human.Explorer, 1);

		//   And: Card played
		await new TradeSuffers().ActAsync(1);

		//   And: City destroyed (like during ravage)
		space.InitDefault(Human.City,0);

		// When: building
		await space.When_CardBuilds();

		// Then: city built
		space.Summary.ShouldBe("1E@1,1T@2");

	}

	[Fact]
	public async Task Level3_DoesntForceCityReplace() {

		var gs = new SoloGameState();
		var coastalSpace = gs.Board[1].ScopeSpace;

		coastalSpace.InitDefault(Human.City,1);

		await new TradeSuffers().When_InvokingLevel(3, user => {
			//  And: user selects a1
			user.Choose(coastalSpace.Label);
			//  And: user choses not to replace (this is what we are testing...)
			user.Choose("Done");
		});

		// Then:
		coastalSpace.Summary.ShouldBe("1C@3");

	}

}

