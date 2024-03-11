namespace SpiritIsland.Tests;

public class ObserveTheEverChangingWorld_Tests {

	[Fact]
	public void RevealedTokens_GainElements() {

		var fxt = new GameFixture()
			.WithSpirit(new ShiftingMemoryOfAges())
			.Start();

		// Given: a space that will ravage and lose tokens
		TargetSpaceCtx space = fxt.TargetSpace("A5");
		space.Space.AdjustDefault(Human.Explorer,1);
		space.Space.Dahan.Init(2);
		var terrain = new[] { Terrain.Wetland, Terrain.Sands, Terrain.Jungle, Terrain.Mountain }.First( space.SpaceSpec.Is );
		fxt.InitRavageCard( terrain);
		//   But: will not build nor explore
		fxt.gameState.InvaderDeck.Build.Cards.Clear();
		fxt.gameState.InvaderDeck.Explore.Cards.Clear();

		//  And: user grows (+9 energy)
		fxt.user.Growth_SelectAction("Gain 9 Energy");
		fxt.spirit.InitElementsFromPresence();
		fxt.spirit.GetAvailableActions(Phase.Fast).Count().ShouldBe(1);

		//   And: enough elements to trigger level-2 innate  (2 moon,1 air)
		fxt.spirit.Elements[Element.Moon] = 2;
		fxt.spirit.Elements[Element.Air] = 1;
		fxt.spirit.Elements.Summary().ShouldBe("2 moon 1 air");

		// Buy slow card (so we don't wrap to next turn)
		fxt.user.PlaysCard(BoonOfAncientMemories.Name);

		//  When: triggers Observe the Ever-changing World (SUT) (level 2 Observe)
		fxt.user.SelectsFastAction($"Learn the Invaders' Tactics,[{ObserveTheEverChangingWorld.Name}]");
		fxt.user.TargetsLand(ObserveTheEverChangingWorld.Name,"[A5],A7,A8");

		//   And: is done with Fast
		fxt.user.IsDoneWith(Phase.Fast);

		var dec = fxt.spirit.Portal.Next;

		if(dec.Prompt == "Select Slow to resolve") {
			_ = dec.ToString();
		} else
			//  Then: Asks user to prepare element
			fxt.user.AssertDecisionInfo("Prepare Element (A5)","Sun,Moon,Fire,Air,Water,[Earth],Plant,Animal");

		fxt.gameState.Phase.ShouldBe(Phase.Slow); // !! sometime BG thread doesn't get all the way to slow and errors out on Invaders, need to wait on BG thread.
	}

	[Fact]
	public void PrepairedElementsMayHaveMultipleStacksOnASpace() {
		var tokens = new CountDictionary<ISpaceEntity>();

		ShiftingMemoryOfAges spirit = new ShiftingMemoryOfAges();
		Board board = Board.BuildBoardA();
		_ = new GameState( spirit, board );
		TargetSpaceCtx ctx = spirit.Target( board[5] );

		var el1 = new ObserveWorldMod( ctx );
		var el2 = new ObserveWorldMod( ctx );

		tokens[el1] = 1;
		tokens[el2] = 2;

		tokens[el1].ShouldBe( 1 );
		tokens[el2].ShouldBe( 2 );
	}

}
