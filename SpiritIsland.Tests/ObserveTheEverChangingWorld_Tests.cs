using SpiritIsland.JaggedEarth;

namespace SpiritIsland.Tests {

	public class ObserveTheEverChangingWorld_Tests {

		[Fact]
		public void RevealedTokens_GainElements() {

			var fxt = new GameFixture()
				.WithSpirit(new ShiftingMemoryOfAges())
				.Start();

			// Given: a space that will ravage and lose tokens
			TargetSpaceCtx space = fxt.TargetSpace("A5");
			space.Tokens.AdjustDefault(Invader.Explorer,1);
			space.Tokens.Dahan.Init(2);
			var terrain = new[] { Terrain.Wetland, Terrain.Sand, Terrain.Jungle, Terrain.Mountain }.First( space.Space.Is );
			fxt.InitRavageCard( terrain);
			//   But: will not build nor explore
			fxt.gameState.InvaderDeck.Build.Cards.Clear();
			fxt.gameState.InvaderDeck.Explore.Cards.Clear();

			//  And: user grows (+9 energy)
			fxt.user.Growth_SelectAction("GainEnergy(9)");
			fxt.spirit.InitElementsFromPresence();
			fxt.spirit.GetAvailableActions(Phase.Fast).Count().ShouldBe(1);

			//   And: enough elements to trigger level-2 innate  (2 moon,1 air)
			fxt.spirit.Elements[Element.Moon] = 2;
			fxt.spirit.Elements[Element.Air] = 1;
			fxt.spirit.Elements.BuildElementString().ShouldBe("2 moon air");

			// Buy slow card (so we don't wrap to next turn)
			fxt.user.PlaysCard(BoonOfAncientMemories.Name);

			//  When: triggers Observe the Ever-changing World (SUT) (level 2 Observe)
			fxt.user.SelectsFastAction($"Learn the Invaders' Tactics,({ObserveTheEverChangingWorld.Name})");
			fxt.user.TargetsLand(ObserveTheEverChangingWorld.Name,"(A5),A7,A8");

			//   And: is done with Fast
			fxt.user.IsDoneWith(Phase.Fast);

			var dec = fxt.spirit.Gateway.GetCurrent();

			if(dec.Prompt == "Select Slow to resolve:") {
				_ = dec.ToString();
			} else
				//  Then: Asks user to prepare element
				fxt.user.AssertDecisionX("Prepare Element (A5)","Sun,Moon,Fire,Air,Water,(Earth),Plant,Animal");

			fxt.gameState.Phase.ShouldBe(Phase.Slow); // !! sometime BG thread doesn't get all the way to slow and errors out on Invaders, need to wait on BG thread.
		}
	}

}
