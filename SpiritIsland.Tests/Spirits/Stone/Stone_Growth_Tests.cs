namespace SpiritIsland.Tests.Spirits.StoneNS;

public class Stone_Growth_Tests {

	[Fact]
	public async Task Presence_Place_Draws_Cards() {
		var spirit = new StonesUnyieldingDefiance();
		var board = Boards.A;
		var gs = new SoloGameState(spirit, board) {
			MinorCards = new PowerCardDeck(typeof(RiversBounty).ScanForMinors(), 1, PowerType.Minor)
		};
		gs.Initialize();

		// Given: first energy already revealed
		await spirit.Presence.Energy.Given_RevealedNextAsync();
	
		// When: user places precence during growth.
		await spirit.DoGrowth(gs).AwaitUser(u => {
			u.NextDecision.HasPrompt("Select Growth").Choose("Place Presence(2)");
			u.NextDecision.HasPrompt("Select Presence to place").Choose("Energy+DrawMinor+CardPlay");
			u.NextDecision.HasPrompt("Where would you like to place your presence?").Choose("A1");
			// Then: user draws power card
			u.NextDecision.HasPrompt("Select minor Power Card").Choose("Uncanny Melting $1 (Slow)");
		}).ShouldComplete();

	}

	// Presence placed during Fast/Slow action - allows card draw
	[Fact]
	public async Task GrowthThroughSacrifice_DrawsCard() {
		var spirit = new StonesUnyieldingDefiance();
		var board = Boards.A;
		var gs = new SoloGameState(spirit, board) {
			MinorCards = new PowerCardDeck(typeof(RiversBounty).ScanForMinors(), 1, PowerType.Minor)
		};
		gs.Initialize();

		// Given: first energy already revealed
		await spirit.Presence.Energy.Given_RevealedNextAsync();

		// When: spirit plays Growth Through Sacrifice
		await spirit.When_ResolvingCard<GrowthThroughSacrifice>().AwaitUser(u => {
			u.NextDecision.HasPrompt("Select Presence to Destroy").Choose("SUD on A4");
			u.NextDecision.HasPrompt("Select location to Remove Blight OR Add Presence").Choose("A1");
			u.NextDecision.HasPrompt("Select Power Option").Choose("Add 1 presence to one of your lands");
			u.NextDecision.HasPrompt("Select Presence to place").Choose("Energy+DrawMinor+CardPlay");
			u.NextDecision.HasPrompt("Select minor Power Card").Choose("Uncanny Melting $1 (Slow)");
		}).ShouldComplete();

	}

	// Stubborn Solidity has visual indication.

}