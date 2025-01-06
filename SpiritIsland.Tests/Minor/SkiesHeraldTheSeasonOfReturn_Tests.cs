namespace SpiritIsland.Tests.Minor;

public class SkiesHeraldTheSeasonOfReturn_Tests {

	[Trait("SpecialRule","ForbiddenGround")]
	[Fact]
	public async Task ReturningKeeperPresence_PushesDahan() {
		var gs = new SoloGameState( new Keeper(), Boards.A);
		SpaceSpec spaceSpec = gs.Board[5];
		Space space = gs.Tokens[spaceSpec];
		var dahanDestination = spaceSpec.Adjacent_Existing.First();

		// Given: Keeper has a destroyed presence
		gs.Spirit.Presence.Destroyed.Count = 1;

		//   And: a presence on target space.
		gs.Spirit.Given_IsOn( space );

		//   And: Dahan on space
		spaceSpec.ScopeSpace.Given_HasTokens("1D@2");

		//  When: play the card
		await PowerCard.ForDecorated(SkiesHeraldTheSeasonOfReturn.ActAsync).ActivateAsync( gs.Spirit ).AwaitUser( u => { 
			// target space
			u.NextDecision.Choose(space);
			// Then: Should Push Dahan (per keeper's Sacred Site)
			u.NextDecision.HasPrompt("Push (1)").ChooseFrom("D@2").ChooseTo(dahanDestination.Label);
			//  And: May Gather up to 2 dahan (per the card)
			u.NextDecision.HasPrompt("Gather up to (1)").ChooseFrom("D@2");
		}).ShouldComplete();

	}

}
