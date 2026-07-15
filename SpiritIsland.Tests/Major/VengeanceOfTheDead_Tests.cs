namespace SpiritIsland.Tests.Major;

public class VengeanceOfTheDead_Tests {

	[Fact]
	public async Task PlayingCard_Grants3Fear() {
		var gs = new SoloGameState();
		var space = gs.Board[2];

		await VengeanceOfTheDead.ActAsync( gs.Spirit.Target(space) ).ShouldComplete("VotD");

		int actualFear = gs.Fear.ActivatedCards.Count * 4 + gs.Fear.EarnedFear;
		actualFear.ShouldBe(3);
	}

	[Fact]
	public async Task DestroyingTownInTargetLand_DealsVengeanceDamageThere() {
		var gs = new SoloGameState();
		var space = gs.Board[2];

		// Given: a Town and an Explorer in target land
		space.Given_HasTokens("1T@2,1E@1");

		var ctx = gs.Spirit.Target(space);

		// When: card played
		await VengeanceOfTheDead.ActAsync(ctx).ShouldComplete("VotD-play");

		//   And: the Town is destroyed (by some other effect) in target land
		await ctx.Invaders.DestroyNOfClass(1, Human.Town).AwaitUser(user => {
			// Then: prompted which land gets vengeance damage (only target land qualifies)
			user.NextDecision.ChooseFirst();
			//   And: prompted which invader receives the 1 damage
			user.NextDecision.ChooseFirst();
		}).ShouldComplete("destroy-town");

		//  Then: the remaining Explorer (1 health) is destroyed by the vengeance damage
		space.Assert_HasInvaders("");
	}

	[Fact]
	public async Task WithThreeAnimal_CanApplyDamageToAdjacentLand() {
		var gs = new SoloGameState();
		var targetSpace = gs.Board[2];
		var ctx = gs.Spirit.Target(targetSpace);
		Space adjacentSpace = ctx.Adjacent.First();

		//   And: spirit has 3 animal, so vengeance damage may reach adjacent lands
		gs.Spirit.Configure().Elements("3 animal");

		// Given: only a Town in target land - destroying it leaves target land with no invaders
		targetSpace.Given_HasTokens("1T@2");
		//   And: an Explorer in the adjacent land
		adjacentSpace.Given_HasTokens("1E@1");

		// When: card played
		await VengeanceOfTheDead.ActAsync(ctx).AwaitUser(user => {
			// Then: prompted whether to activate the 3-animal threshold
			user.NextDecision.Choose("Yes");
		}).ShouldComplete("VotD-play");

		//   And: the Town is destroyed
		await ctx.Invaders.DestroyNOfClass(1, Human.Town).AwaitUser(user => {
			// Then: target land no longer has invaders, so the adjacent land is the only valid choice
			user.NextDecision.ChooseFirst();
			user.NextDecision.ChooseFirst();
		}).ShouldComplete("destroy-town-adjacent");

		//  Then: the adjacent Explorer is destroyed by the vengeance damage
		adjacentSpace.Assert_HasInvaders("");
	}

}
