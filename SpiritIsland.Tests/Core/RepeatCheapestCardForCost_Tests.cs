namespace SpiritIsland.Tests.Core;

[Collection("BaseGame Spirits")]
public sealed class RepeatCheapestCardForCost_Tests {

	[Fact]
	public async Task NothingToRepeat_OffersNoCards_AndCompletes() {
		// Given: a spirit that has not used any cards yet
		var spirit = new VitalStrength();
		var gs = new SoloGameState( spirit, Boards.A );
		gs.Phase = Phase.Fast;
		var repeat = new RepeatCheapestCardForCost();

		// Then: there are no options (used to throw "Sequence contains no elements")
		repeat.GetCardOptions( spirit, Phase.Fast ).ShouldBeEmpty();

		// And: using it does nothing, without prompting
		await repeat.ActivateAsync( spirit ).ShouldComplete();
	}
}
