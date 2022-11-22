using SpiritIsland.JaggedEarth;

namespace SpiritIsland.Tests.JaggedEarth;

public class SkiesHeraldTheSeasonOfReturn_Tests {

	[Fact]
	public void ReturningKeeperPresence_PushesDahan() {
		var fix = new ConfigurableTestFixture { Spirit = new Keeper() };
		var space = fix.Board[5];
		var spaceState = fix.GameState.Tokens[space];
		var dahanDestination = space.Adjacent.First();

		// Given: Keeper has a destroyed presence
		fix.Spirit.Presence.Destroyed = 1;

		//   And: a presence on target space.
		fix.Spirit.Presence.Adjust(spaceState,1);

		//   And: Dahan on space
		fix.InitTokens(space,"1D@2");

		//  When: play the card
		var task = PowerCard.For<SkiesHeraldTheSeasonOfReturn>().ActivateAsync( fix.SelfCtx );
		// target space
		fix.Choose(space);

		// Then: Should Push Dahan (per keeper's Sacred Site)
		fix.Choose("D@2");
		fix.Choose( dahanDestination );
		//  And: May Gather up to 2 dahan (per the card)
		fix.Choose("D@2 on "+ dahanDestination.Text ); // "Gather up to 2 Dahan"
		//  And: May push 1 blight
		// no blight to push

		task.IsCompletedSuccessfully.ShouldBeTrue();
	}

}
