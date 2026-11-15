namespace SpiritIsland.Tests.Invaders;

/// <summary>
/// InvaderPhase.StepAsync runs the phase one step at a time with an InvaderCursor, so a game can be saved
/// between steps and resumed without replaying the steps already done.
/// </summary>
public class InvaderPhase_Cursor_Tests {

	// Only one game can be live at a time (GameState.Current/ActionScope are ambient), so each test builds
	// its games one after another.
	static GameState NewGameState() {
		GameState gs = new GameConfiguration().ConfigSpirits( VitalStrength.Name ).ConfigBoards( "A" ).BuildGame();
		ActionScope.Initialize( gs.RootScope );
		return gs;
	}

	static string Json( GameState gs ) => gs.ToJson( new GameStateSerializationContext( gs ) ).ToJsonString();

	static async Task<string> RunWholePhase() {
		GameState gs = NewGameState();
		await InvaderPhase.ActAsync( gs );
		return Json( gs );
	}

	[Fact]
	public async Task SteppingThroughThePhase_GivesSameResultAsActAsync() {
		string expected = await RunWholePhase();

		GameState gs = NewGameState();
		var cursor = new InvaderCursor();
		int steps = 0;
		while( await InvaderPhase.StepAsync( gs, cursor ) ) ++steps;

		steps.ShouldBeGreaterThan( 3 ); // really did go step by step
		cursor.Step.ShouldBe( InvaderStep.PreInvader ); // back at the start, ready for the next round
		Json( gs ).ShouldBe( expected );
	}

	[Fact]
	public async Task SavedBetweenSteps_ResumesWithoutReplayingEarlierSteps() {
		string expected = await RunWholePhase();

		// Run partway: pre-invader, fear (none activated), then Ravage (empty in round 1) and Build.
		GameState gs = NewGameState();
		string before = Json( gs );
		var cursor = new InvaderCursor();
		for( int i = 0; i < 4; ++i )
			(await InvaderPhase.StepAsync( gs, cursor )).ShouldBeTrue();
		cursor.Step.ShouldBe( InvaderStep.Slots );
		cursor.SlotIndex.ShouldBe( 2 );
		string midPhase = Json( gs );
		midPhase.ShouldNotBe( before ); // Build really changed the board
		string cursorOnDisk = cursor.ToJson().ToJsonString();
		string gameOnDisk = midPhase;

		// "Restart": a fresh game restored from the text picks up at the Explore slot, not back at Ravage.
		GameState resumed = NewGameState();
		resumed.RestoreFromJson( JsonNode.Parse( gameOnDisk )!.AsObject(), new GameStateSerializationContext( resumed ) );
		var resumedCursor = new InvaderCursor();
		resumedCursor.RestoreFromJson( JsonNode.Parse( cursorOnDisk )!.AsArray() );
		while( await InvaderPhase.StepAsync( resumed, resumedCursor ) ) { }

		Json( resumed ).ShouldBe( expected );
	}

}
