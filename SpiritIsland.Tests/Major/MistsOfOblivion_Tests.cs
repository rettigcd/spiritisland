namespace SpiritIsland.Tests.Major;

// Note: destroying a Town/City always generates base-game fear (InvaderBinding: Town=1, City=2)
// independent of any card. Expected totals below include that base fear on top of the card's own
// "1 fear per Town/City destroyed (max 4)" effect.
public class MistsOfOblivion_Tests {

	[Fact]
	public async Task DestroyingTown_GrantsCardFearPlusBaseFear() {
		var gs = new SoloGameState();
		var space = gs.Board[2];
		space.Given_HasTokens("1T@2");

		var ctx = gs.Spirit.Target(space);

		// When: card played
		await MistsOfOblivion.ActAsync(ctx).ShouldComplete("MoO-play");

		//   And: the town is destroyed (by some other effect) within the same action
		await ctx.Invaders.DestroyNOfClass(1, Human.Town).ShouldComplete("destroy-town");

		// Then: 1 fear from the card + 1 base fear (Town.FearGeneratedWhenDestroyed)
		int actualFear = gs.Fear.ActivatedCards.Count * 4 + gs.Fear.EarnedFear;
		actualFear.ShouldBe(2);
	}

	[Fact]
	public async Task DestroyingTwoTownsInOneEvent_Grants2CardFear() {
		var gs = new SoloGameState();
		var space = gs.Board[2];
		space.Given_HasTokens("2T@2");

		var ctx = gs.Spirit.Target(space);

		// When: card played
		await MistsOfOblivion.ActAsync(ctx).ShouldComplete("MoO-play");

		//   And: both towns are destroyed together in a single removal event
		await ctx.Invaders.DestroyNOfClass(2, Human.Town).ShouldComplete("destroy-towns");

		// Then: 2 fear from the card (1 per town, not 1 per event) + 2 base fear (1/town)
		int actualFear = gs.Fear.ActivatedCards.Count * 4 + gs.Fear.EarnedFear;
		actualFear.ShouldBe(4);
	}

	[Fact]
	public async Task DestroyingMoreThan4Towns_CapsCardFearAt4() {
		var gs = new SoloGameState();
		var space = gs.Board[2];
		space.Given_HasTokens("5T@2");

		var ctx = gs.Spirit.Target(space);

		await MistsOfOblivion.ActAsync(ctx).ShouldComplete("MoO-play");

		// destroy 1 at a time so each is its own removal event
		for(int i = 0; i < 5; ++i)
			await ctx.Invaders.DestroyNOfClass(1, Human.Town).ShouldComplete($"destroy-town-{i}");

		// Then: card fear capped at 4, but base fear (1/town) applies to all 5 regardless of the cap
		int actualFear = gs.Fear.ActivatedCards.Count * 4 + gs.Fear.EarnedFear;
		actualFear.ShouldBe(4 + 5);
	}

	[Fact]
	public async Task DestroyingTownInALaterAction_GrantsOnlyBaseFear() {
		var gs = new SoloGameState();
		var space = gs.Board[2];
		space.Given_HasTokens("1T@2");

		// When: card played in one action
		await using(var scope1 = await ActionScope.StartSpiritAction(ActionCategory.Spirit_Power, gs.Spirit)) {
			await MistsOfOblivion.ActAsync( gs.Spirit.Target(space) ).ShouldComplete("MoO-play");
		}

		//   And: the town is destroyed in a later, separate action
		await using(var scope2 = await ActionScope.StartSpiritAction(ActionCategory.Spirit_Power, gs.Spirit)) {
			await gs.Spirit.Target(space).Invaders.DestroyNOfClass(1, Human.Town).ShouldComplete("destroy-later");
		}

		// Then: the card's mod only applies within the action it was created in, so only the
		// unavoidable base fear (1 per Town) is generated - not the card's bonus fear
		int actualFear = gs.Fear.ActivatedCards.Count * 4 + gs.Fear.EarnedFear;
		actualFear.ShouldBe(1);
	}

}
