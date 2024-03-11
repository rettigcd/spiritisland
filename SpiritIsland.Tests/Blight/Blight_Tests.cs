using SpiritIsland.Log;

namespace SpiritIsland.Tests.Blight;

public class Blight_Tests {

	[Trait("Blight","FlipCard")]
	[Fact]
	public async Task Taking3Blight_FlipsCard() {
		Spirit spirit = new RiverSurges();
		Board board = Board.BuildBoardD();
		GameState gs = new GameState(spirit,board);
		Task<IslandBlighted> waitForBlightedIsland = gs.WatchForBlightedIsland();
		gs.Initialize();

		// Given:
		gs.BlightCard.ShouldNotBeNull();
		gs.BlightCard = new InvadersFindTheLandToTheirLiking();
		BlightCard.Space.ScopeSpace.Init(Token.Blight,3);
		gs.BlightCard.CardFlipped.ShouldBeFalse();

		// When: Taking Blight from Card
		await gs.TakeBlightFromCard(3).AwaitUser( async (u) => {
			(await waitForBlightedIsland).Card.Text.ShouldBe( "Invaders Find the Land to Their Liking" );
		} ).ShouldComplete("Taking Blight From Card");

		// Then:
		gs.BlightCard.CardFlipped.ShouldBeTrue();

	}

	[Fact]
	public async Task TwoBlight_TriggerBlightedIslandOnce(){
		Spirit spirit = new LureOfTheDeepWilderness(); // any will do
		Board board = Board.BuildBoardC();
		GameState gs = new GameState(spirit,board);
		Task<IslandBlighted> waitForBlightedIsland = gs.WatchForBlightedIsland();
		gs.Initialize();

		// Given: Blight Card will be "Promising Farmlands" (this card is easy to test...)
		gs.BlightCard = new PromisingFarmlands();
		//   And: We are on our last blight
		BlightCard.Space.ScopeSpace.Init(Token.Blight,1);

		//   And: Next ravage is Coastal
		gs.InvaderDeck.Ravage.Cards.Add( InvaderCard.Stage2Costal() );

		//   And: Blight will Trigger on 2 Spaces.   (Town/City on C1/C2)
		board[1].Given_InitSummary("1T@2");
		board[2].Given_InitSummary("1C@3");
		board[3].Given_InitSummary("");

		//  When: Ravaging causes 2 blights
		await InvaderPhase.ActAsync(gs).AwaitUser(async user=>{

			//  Then: "Promising Farmlands" triggers ONLY ONCE
			(await waitForBlightedIsland).Card.Text.ShouldBe("Promising Farmlands");
			user.NextDecision.HasPrompt("Select space to Add 1 Town  Add 1 City").HasOptions("C4,C5,C6,C8").Choose("C4");

		}).ShouldComplete("Invader Phase");

		// And: we have blight on both 1 & 2
		board[1].ScopeSpace.Blight.Count.ShouldBe(1);
		board[2].ScopeSpace.Blight.Count.ShouldBe(1);
	}

}