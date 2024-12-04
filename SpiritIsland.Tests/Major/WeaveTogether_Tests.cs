namespace SpiritIsland.Tests.Major;

public class WeaveTogether_Tests {

	[Fact]
	public async Task SwapsOutBoardSpaces() {
		var gs = new SoloGameState(new LureOfTheDeepWilderness(), Boards.B);

		// When
		Space joined = await When_WeaveTogether(gs, gs.Board[2], gs.Board[4]);

		// Then: joined space should be in the board
		joined.ShouldNotBeNull();

		//  And: original spaces are removed
		gs.Spaces.SingleOrDefault(s => s.Label == "B2").ShouldBeNull();
		gs.Spaces.SingleOrDefault(s => s.Label == "B4").ShouldBeNull();

	}

	[Fact]
	public async Task HasBothTerrains() {
		var gs = new SoloGameState(new LureOfTheDeepWilderness(), Boards.B);
		var jungle = gs.Board[8];
		var wetland = gs.Board[6];

		// When
		Space joined = await When_WeaveTogether(gs, jungle, wetland);

		// Then: joined space should be both Terrains
		joined.SpaceSpec.IsJungle.ShouldBeTrue();
		joined.SpaceSpec.IsWetland.ShouldBeTrue();
	}

	[Fact]
	public async Task JoiningCoastlandWithInland_BecomesOneBigCoastal() {
		// See "joining a Coastal land with an Inland land results in one big Coastal land"
		// from https://querki.net/raw/darker/spirit-island-faq/Weave+Together+the+Fabric+of+Place

		// Coastal and Inland are positional, not terrain-based
		// so they get their value purely from their adjacency to an ocean.

		var gs = new SoloGameState(new LureOfTheDeepWilderness(), Boards.B);
		var inland = gs.Board[4];
		var coastal = gs.Board[2];

		// When
		Space joined = await When_WeaveTogether(gs, inland, coastal);

		// Then: space is coastal
		joined.SpaceSpec.IsCoastal.ShouldBeTrue();

		//  And: not inland
		TerrainMapper.Current.IsInland(joined).ShouldBeFalse();
	}

	[Fact]
	public async Task JoiningOceanWithCoastal_BecomesCoastalAndMakesMoreCoastals() {
		// See "joining a Coastal land with an Inland land results in one big Coastal land"
		// from https://querki.net/raw/darker/spirit-island-faq/Weave+Together+the+Fabric+of+Place

		// Coastal and Inland are positional, not terrain-based
		// so they get their value purely from their adjacency to an ocean.
		var gs = new SoloGameState(new Ocean(), Boards.B);
		var ocean = gs.Board[0].ScopeSpace;
		var coastal = gs.Board[2];
		var inland = gs.Board[4];

		// When: joining ocean to coastal
		var joined = await When_WeaveTogether(gs, ocean.SpaceSpec, coastal);

		// Then: space is coastal and ocean
		joined.SpaceSpec.IsCoastal.ShouldBeTrue();
		joined.SpaceSpec.IsOcean.ShouldBeTrue();

		//  And: old inland space is now coastal.
		joined.SpaceSpec.IsCoastal.ShouldBeTrue();
		//  And: no longer inland
		TerrainMapper.Current.IsInland(joined).ShouldBeFalse();
	}

	static async Task<Space> When_WeaveTogether(SoloGameState gs,SpaceSpec targetSpace, SpaceSpec otherSpace) {
		// When: Weave Together played on B4 & B2
		await WeaveTogetherTheFabricOfPlace.ActAsync(gs.Spirit.Target(targetSpace)).AwaitUser(user => {
			user.NextDecision.HasPrompt($"Join {targetSpace.Label} to").Choose(otherSpace.Label);
		});

		// Then joined space is part of board
		string joinedLabel = string.Join(":",new SpaceSpec[]{targetSpace,otherSpace}.OrderBy(s => s.Label));
		return gs.Spaces_Unfiltered.SingleOrDefault(s => s.Label == joinedLabel)
			?? throw new Exception("Unable to find joined spaces " + joinedLabel);
	}
}
