using Shouldly;
using SpiritIsland.JaggedEarth;
using System.Linq;
using Xunit;

namespace SpiritIsland.Tests {

	public class ObserveTheEverChangingWorld_Tests {

		[Fact]
		public void RevealedTokens_GainElements() {

			var fxt = new GameFixture()
				.WithSpirit(new ShiftingMemoryOfAges())
				.Start();

			// Given: a space that will ravage and lose tokens
			var space = fxt.TargetSpace("A5");
			space.Tokens.Adjust(Invader.Explorer.Default,1);
			space.Tokens.Dahan.Add(2);
			fxt.gameState.InvaderDeck.Ravage.Add(new InvaderCard(space.Terrain));
			//   But: will not build nor explore
			fxt.gameState.InvaderDeck.Build.Clear();
			fxt.gameState.InvaderDeck.Explore.Clear();

			//   And: enough elements to trigger level-2 innate  (2 moon,1 air)
			fxt.spirit.Presence.Energy.slots[1] = new Track("2 moon,1 air",Element.Moon,Element.Moon,Element.Air);
			++fxt.spirit.Presence.Energy.RevealedCount;
			fxt.spirit.InitElementsFromPresence();

			// When: user grows (+9 energy)
			fxt.user.Growth_SelectsOption(3);
			fxt.spirit.Elements.BuildElementString().ShouldBe("2 moon 1 air");
			fxt.spirit.GetAvailableActions(Phase.Fast).Count().ShouldBe(1);

			// Buy slow card (so we don't wrap to next turn)
			fxt.user.PlaysCard(BoonOfAncientMemories.Name);

			//  When: triggers SUT (level 2 Observe)
			fxt.user.SelectsFastAction($"Learn the Invaders' Tactics,({ObserveTheEverChangingWorld.Name})");
			fxt.user.TargetsLand(ObserveTheEverChangingWorld.Name,"(A5),A7,A8");

			//   And: is done with Fast
			fxt.user.IsDoneWith(Phase.Fast);

			var dec = fxt.spirit.Action.GetCurrent();

			if(dec.Prompt == "Select Slow to resolve:") {
				_ = dec.ToString();
			} else
				//  Then: Asks user to prepare element
				fxt.user.AssertDecisionX("Prepare Element (A5)","Sun,Moon,Air,Fire,Water,(Earth),Plant,Animal");

			fxt.gameState.Phase.ShouldBe(Phase.Slow);
		}
	}

}
