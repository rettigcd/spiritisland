using Shouldly;
using SpiritIsland.Basegame;
using SpiritIsland.BranchAndClaw.Minor;
using Xunit;

namespace SpiritIsland.Tests {

	public class GrowthThroughSacrifice_Tests {

		[Fact]
		public void RevealedTokens_GainElements() {

			var fxt = new GameFixture()
				.WithSpirit(new TestSpirit(PowerCard.For<GrowthThroughSacrifice>()))
				.Start();

			fxt.spirit.AddCardToHand(PowerCard.For<GnawingRootbiters>());

			// Given: Spirits first energy track has 1-fire
			fxt.spirit.Presence.Energy.slots[1] = Track.FireEnergy;
			// Given: Spirits can play 2 cards
			fxt.spirit.Presence.CardPlays.slots[0] = Track.Card2;
			// Given: Spirit has 2 presence on A5
			fxt.spirit.Presence.PlaceOn(fxt.gameState.Island.Boards[0][5],fxt.gameState);

			// When: user grows
			fxt.user.Growth_SelectsOption(0);
			//  And: plays card SUT
			fxt.user.PlaysCard(GrowthThroughSacrifice.Name);
			//  And: plays slow card (to keep round from ending)
			fxt.user.PlaysCard(GnawingRootbiters.Name);

			fxt.spirit.Elements[Element.Fire].ShouldBe(1); // from Growth Through Sacrifice card

			//  And: Resolves SUT
			fxt.user.SelectsFastAction(GrowthThroughSacrifice.Name);
			fxt.user.AssertDecisionX("Select presence to destroy","A5");

			fxt.user.AssertDecisionX("Select Power Option","Remove 1 blight from one of your lands,(Add 1 presence to one of your lands)");
			fxt.user.AssertDecisionX("Select Presence to place.","(fire energy),2 cardplay,Take Presence from Board");
			fxt.user.AssertDecisionX("Where would you like to place your presence?","A5");

			// Should be waiting on slow action
			fxt.spirit.Elements[Element.Fire].ShouldBe(2); // 1 from Growth Through Sacrifice card, 1 from Energy track
		}

	}

}
