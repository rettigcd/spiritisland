namespace SpiritIsland.Tests.BranchAndClaw;

public class Token_Tests {

	static bool IsInPlay(Space space) => !space.IsOcean;

	[Fact]
	public void Wilds_Stops_Explore() {
		var gs = new GameState( new Thunderspeaker(), Board.BuildBoardC());

		// Given: a space with no invaders
		SpaceState spaceState = gs.AllSpaces.First( s=>IsInPlay(s.Space) && !s.HasInvaders() );
		//   And: 1 neighboring town
		var neighbor = spaceState.Adjacent.First();
		neighbor.AdjustDefault(Invader.Town,1);
		//   And: 1 wilds there
		spaceState.Wilds.Init(1);

		//  When: we explore there
		_ = InvaderCardEx.For( spaceState.Space ).Explore( gs );

		//  Then: still no invaders
		spaceState.HasInvaders().ShouldBeFalse("there should be no explorers in "+spaceState.Space.Label);
		//   And: no wilds here
		(spaceState.Wilds.Count>0).ShouldBeFalse("wilds should be used up");

	}

	[Fact]
	public async Task Disease_Stops_Build() {
		var gs = new GameState( new Thunderspeaker(), Board.BuildBoardC() );

		// Given: a space with ONLY 1 explorer
		SpaceState space = gs.AllSpaces.First( s => IsInPlay(s.Space) && !s.HasInvaders() ); // 0 invaders
		space.AdjustDefault( Invader.Explorer, 1 ); // add explorer
		//   And: 1 diseases there
		await space.Disease.Bind(Guid.NewGuid()).Add(1);

		//  When: we build there
		await InvaderCardEx.For( space.Space ).Build( gs );

		//  Then: still no towns (just original explorer)
		gs.Assert_Invaders(space.Space, "1E@1" ); // "should be no town on "+space.Label
		//   And: no disease here
		space.Disease.Any.ShouldBeFalse( "disease should be used up" );

	}

}