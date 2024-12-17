namespace SpiritIsland.Tests.Minor;

public class SkyStretchesToShore_Tests {

	[Trait("speed","change")]
	[Fact]
	public async Task SlowAsFast() {
		var gs = new SoloGameState();

		// Given: spirit has a Range-1 slow card && SkyStreches to shore
		var slow = PowerCard.ForDecorated(GnawingRootbiters.ActAsync);
		var sut = PowerCard.ForDecorated(SkyStretchesToShore.ActAsync);
		gs.Spirit.AddActionFactory(slow);
		gs.Spirit.AddActionFactory(sut);

		//  And: Can resolve the slow card 
		gs.Spirit.Given_IsOn(gs.Board[8]);

		//   And: in fast phase
		gs.Phase = Phase.Fast;

		await gs.Spirit.SelectAndResolveActions(gs).AwaitUser(user => {

			//  When: spirit activates SkyStreteches to Shore
			user.NextDecision.HasPrompt("Select Fast to resolve").HasOptions("Sky Stretches to Shore $1 (Fast),Done").ChooseFirst();

			//  And: decides to resolve slow 
			user.NextDecision.HasPrompt("Select Fast to resolve").HasOptions("Resolve Other Speed Action,Done").ChooseFirst();
			user.NextDecision.HasPrompt("Select action to make fast.").HasOptions("Gnawing Rootbiters $0 (Slow),Done").ChooseFirst();

			//  Then: they can resolve the slow card.
			user.NextDecision.HasPrompt("Gnawing Rootbiters: Target Space")
				// On all shores & all inlands spaces except A4 which is at Range-2
				.HasOptions("A1,A2,A3,A5,A6,A7,A8").ChooseFirst();

		}).ShouldComplete("Fast");

	}

}