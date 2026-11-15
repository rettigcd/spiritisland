using SpiritIsland.Basegame;
using SpiritIsland.SinglePlayer;

namespace SpiritIsland.Tests.Solo;

/// <summary>
/// A prompt inside the Invader phase (here, Fear cards asking the spirit to pick a land) times out: the game
/// rolls back only the step that was prompting, keeps the answers already given to earlier steps, and can be
/// resumed from the saved checkpoint in a fresh game.
/// </summary>
public class SoloGame_InvaderSuspend_Tests {

	/// <summary>
	/// Round 1, at the start of the Invader phase, with 2 Isolation Fear cards activated and 3 lands each
	/// holding a lone Explorer - so the first card has 3 candidate lands to prompt over, the second has 2.
	/// </summary>
	static SoloGame NewGameAtInvaders() {
		GameState gs = new GameConfiguration().ConfigSpirits( VitalStrength.Name ).ConfigBoards( "A" ).BuildGame();
		var game = new SoloGame( gs );
		ActionScope.Initialize( gs.RootScope );

		foreach( int i in new[] { 1, 3 } ) // board[5] already has a lone Explorer
			gs.Tokens[ gs.Island.Boards[0][i] ].AdjustDefault( Human.Explorer, 1 );
		gs.Fear.ActivatedCards.Push( new Isolation() );
		gs.Fear.ActivatedCards.Push( new Isolation() );

		// Jump the cursor to the Invader phase (RoundStep.Invaders == 5) by loading an edited save.
		JsonObject snapshot = game.Save();
		snapshot["RoundStep"] = 5;
		snapshot["Phase"] = (int)Phase.Invaders;
		game.Load( snapshot );
		return game;
	}

	/// <summary> Answers the prompt the engine is waiting on with its first option. </summary>
	static void AnswerFirst( SoloGame game ) => game.Spirit.NextDecision().ChooseFirst();

	[Fact]
	public async Task PromptTimeoutInSecondFearCard_KeepsFirstAnswer_AndResumesIdentically() {

		// Baseline: answer both Fear card prompts, nothing times out.
		SoloGame uninterrupted = NewGameAtInvaders();
		Task<PendingMenu> running = uninterrupted.AdvanceAsync();
		AnswerFirst( uninterrupted ); // card 1
		AnswerFirst( uninterrupted ); // card 2
		await running;
		string expected = uninterrupted.Save().ToJsonString();

		// Same game, but nobody answers the second card's prompt.
		SoloGame game = NewGameAtInvaders();
		game.PromptTimeout = TimeSpan.FromMilliseconds( 300 );
		running = game.AdvanceAsync();
		AnswerFirst( game ); // card 1
		PendingMenu result = await running;

		result.Kind.ShouldBe( MenuKind.Suspended );
		// Rolled back to between the two cards: card 1's answer is kept, card 2 still waiting.
		JsonArray remainingCards = result.Checkpoint!["Fear"]!["ActivatedCards"]!.AsArray();
		remainingCards.Count.ShouldBe( 1 );
		result.Checkpoint["InvaderCursor"]![0]!.GetValue<int>().ShouldBe( (int)InvaderStep.Fear );

		// "Restart the process" from what was written to disk, and answer the card that was left.
		SoloGame resumed = NewGameAtInvaders(); // re-initializes the ambient scope for the new game
		resumed.Load( JsonNode.Parse( result.Checkpoint.ToJsonString() )!.AsObject() );
		running = resumed.AdvanceAsync();
		AnswerFirst( resumed ); // card 2
		await running;

		resumed.Save().ToJsonString().ShouldBe( expected );
	}

}
