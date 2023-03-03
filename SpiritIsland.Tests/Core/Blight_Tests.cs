namespace SpiritIsland.Tests.Core;

public class Blight_Tests {

	[Trait("Blight","FlipCard")]
	[Fact]
	public async Task Taking3Blight_FlipsCard() {
		Spirit spirit = new RiverSurges();
		Board board = Board.BuildBoardD();
		GameState gs = new GameState(spirit,board);
		gs.Initialize();

		// Given:
		gs.BlightCard.ShouldNotBeNull();
		gs.BlightCard = new InvadersFindTheLandToTheirLiking();
		BlightCard.Space.Tokens.Init(Token.Blight,3);
		gs.BlightCard.CardFlipped.ShouldBeFalse();

		// When: Taking Blight from Card
		await gs.TakeBlightFromCard(3).AwaitUserToComplete("Taking Blight From Card", () => {
			VirtualUser u = new VirtualUser(spirit);
			u.NextDecision.HasPrompt("Island blighted").Choose( "Invaders Find the Land to Their Liking" );
		} );

		// Then:
		gs.BlightCard.CardFlipped.ShouldBeTrue();

	}
}