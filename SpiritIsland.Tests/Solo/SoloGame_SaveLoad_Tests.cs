using SpiritIsland.SinglePlayer;

namespace SpiritIsland.Tests.Solo;

/// <summary>
/// SoloGame.Save()/Load() carry the whole game plus the round-step cursor, so a game can be suspended
/// between actions (driven by AdvanceAsync/ChooseAsync, no long-lived engine task) and resumed on a freshly
/// built SoloGame.
/// </summary>
public class SoloGame_SaveLoad_Tests {

	static SoloGame NewGame() {
		GameState gs = new GameConfiguration().ConfigSpirits( VitalStrength.Name ).ConfigBoards( "A" ).BuildGame();
		var game = new SoloGame( gs );
		ActionScope.Initialize( gs.RootScope );
		return game;
	}

	static SoloGame Resume( JsonObject saved ) {
		SoloGame resumed = NewGame();
		resumed.Load( JsonNode.Parse( saved.ToJsonString() )!.AsObject() ); // through text, as if from disk
		return resumed;
	}

	static string[] Titles( PendingMenu menu ) => [.. menu.Options.Select( a => a.Title )];

	/// <summary> Picks the first menu option; the action then waits on its sub-decision, which we answer. </summary>
	static async Task ResolveWithFirstChoice( SoloGame game, string subDecisionPrompt ) {
		Task<PendingMenu> chosen = game.ChooseAsync( 0 );
		game.Spirit.NextDecision().HasPrompt( subDecisionPrompt ).ChooseFirst();
		await chosen;
	}

	[Fact]
	public async Task SaveThenLoad_OnFreshGame_RestoresCursorAndMenu() {
		SoloGame original = NewGame();
		PendingMenu menu = await original.AdvanceAsync(); // runs RoundStart, stops at the first Growth menu
		menu.Kind.ShouldBe( MenuKind.Growth );
		JsonObject saved = original.Save();

		SoloGame resumed = Resume( saved );

		Titles( await resumed.AdvanceAsync() ).ShouldBe( Titles( menu ) );
		resumed.Save().ToJsonString().ShouldBe( saved.ToJsonString() );
	}

	[Fact]
	public async Task SuspendedMidRound_PlaysOnIdenticallyToUninterruptedGame() {

		// Play straight through: Gain 2 Energy, then Place Presence (which needs a sub-decision answered).
		// Only one SoloGame can be live at a time (GameState.Current/ActionScope are ambient), exactly as
		// in a real process, so the uninterrupted game finishes before the resumed one is built.
		SoloGame uninterrupted = NewGame();
		PendingMenu menu = await uninterrupted.AdvanceAsync();
		int gainEnergy = Array.FindIndex( menu.Options, a => a.Title == "Gain 2 Energy" );
		gainEnergy.ShouldBeGreaterThanOrEqualTo( 0 );
		menu = await uninterrupted.ChooseAsync( gainEnergy );
		JsonObject suspendedAt = uninterrupted.Save(); // between actions - this is what would go to disk
		await ResolveWithFirstChoice( uninterrupted, "Select Presence to place" );
		string expected = uninterrupted.Save().ToJsonString();

		// "Restart the process": a brand-new game resumed from the text on disk plays the same next action.
		SoloGame resumed = Resume( suspendedAt );
		Titles( await resumed.AdvanceAsync() ).ShouldBe( Titles( menu ) );
		await ResolveWithFirstChoice( resumed, "Select Presence to place" );

		resumed.Save().ToJsonString().ShouldBe( expected );
	}

	/// <summary> Gain 2 Energy, then Place Presence (answering its sub-decision) - returns the resulting save. </summary>
	static async Task<string> PlayOpening() {
		SoloGame game = NewGame(); // initializes the ambient ActionScope on the calling flow
		PendingMenu menu = await game.AdvanceAsync();
		await game.ChooseAsync( Array.FindIndex( menu.Options, a => a.Title == "Gain 2 Energy" ) );
		await Task.Yield(); // give the other games a chance to interleave
		await ResolveWithFirstChoice( game, "Select Presence to place" );
		return game.Save().ToJsonString();
	}

	[Fact]
	public async Task GamesRunningInParallel_EachInItsOwnExecutionContext_DoNotInterfere() {
		// Baseline: the same opening played with nothing else running.
		string solo = await Task.Run( PlayOpening );

		// Task.Run gives each game its own copy of the AsyncLocals, so each game's ActionScope.Initialize
		// (inside NewGame) is private to it. Initializing before spawning would share one container.
		string[] parallel = await Task.WhenAll( Enumerable.Range( 0, 4 ).Select( _ => Task.Run( PlayOpening ) ) );

		foreach( string result in parallel )
			result.ShouldBe( solo );
	}

	[Fact]
	public async Task PromptTimeout_InsideAnAction_SuspendsAtTheMenuAndResumesIdentically() {
		string expected = await Task.Run( PlayOpening ); // uninterrupted baseline

		SoloGame game = NewGame();
		game.PromptTimeout = TimeSpan.FromMilliseconds( 200 );
		PendingMenu menu = await game.AdvanceAsync();
		menu = await game.ChooseAsync( Array.FindIndex( menu.Options, a => a.Title == "Gain 2 Energy" ) );
		string[] placePresenceMenu = Titles( menu );
		string atMenu = game.Save().ToJsonString();

		// Place Presence prompts "Select Presence to place" - nobody answers, so it times out.
		PendingMenu result = await game.ChooseAsync( 0 );

		result.Kind.ShouldBe( MenuKind.Suspended );
		game.Suspended.ShouldBe( result.Checkpoint );
		result.Checkpoint!.ToJsonString().ShouldBe( atMenu ); // rolled back to the menu, nothing half-done
		game.Save().ToJsonString().ShouldBe( atMenu );

		// "Restart the process" from what was written to disk.
		SoloGame resumed = Resume( result.Checkpoint );
		Titles( await resumed.AdvanceAsync() ).ShouldBe( placePresenceMenu );
		await ResolveWithFirstChoice( resumed, "Select Presence to place" );
		resumed.Save().ToJsonString().ShouldBe( expected );
	}

	[Fact]
	public void GameConfiguration_RoundTripsThroughJson() {
		var config = new GameConfiguration()
			.ConfigSpirits( VitalStrength.Name, RiverSurges.Name ).ConfigBoards( "A", "B" )
			.ConfigAdversary( "France", 3 ).ConfigCommandBeasts( true );
		config.ShuffleNumber = 42;
		config.Aspects = [new AspectConfigKey( VitalStrength.Name, "Aspect" )];

		var restored = GameConfiguration.FromJson( JsonNode.Parse( config.ToJson().ToJsonString() )!.AsObject() );

		restored.ToJson().ToJsonString().ShouldBe( config.ToJson().ToJsonString() );
		restored.Spirits.ShouldBe( config.Spirits );
		restored.Adversary.ShouldBe( config.Adversary );
		restored.Aspects.ShouldBe( config.Aspects );
		restored.CommandTheBeasts.ShouldBeTrue();
	}

	[Fact]
	public async Task WrittenToFile_ReadBackIntoFreshGame_ResumesIdentically() {
		string path = Path.Combine( Path.GetTempPath(), $"solo-{Guid.NewGuid():N}.json" );
		try {
			var config = new GameConfiguration().ConfigSpirits( VitalStrength.Name ).ConfigBoards( "A" );
			GameState gs = TestGames.GameBuilder.BuildGame( config );
			var game = new SoloGame( gs );
			ActionScope.Initialize( gs.RootScope );

			PendingMenu menu = await game.AdvanceAsync();
			menu = await game.ChooseAsync( Array.FindIndex( menu.Options, a => a.Title == "Gain 2 Energy" ) );
			SoloGameFile.Write( path, config, game );
			File.Exists( path + ".tmp" ).ShouldBeFalse(); // temp file was moved over, not left behind
			await ResolveWithFirstChoice( game, "Select Presence to place" );
			string expected = game.Save().ToJsonString();

			// "Restart the process": nothing but the file and a GameBuilder.
			( SoloGame resumed, GameConfiguration resumedConfig ) = SoloGameFile.Read( path, TestGames.GameBuilder );

			resumedConfig.ToJson().ToJsonString().ShouldBe( config.ToJson().ToJsonString() );
			Titles( await resumed.AdvanceAsync() ).ShouldBe( Titles( menu ) );
			await ResolveWithFirstChoice( resumed, "Select Presence to place" );
			resumed.Save().ToJsonString().ShouldBe( expected );
		}
		finally {
			File.Delete( path );
		}
	}

	[Fact]
	public async Task LatestCheckpoint_WhileEngineWaitsMidAction_IsTheLastMenuAndResumes() {
		SoloGame game = NewGame();
		game.Start(); // the long-lived StartAsync loop, as Maui runs it
		game.Spirit.NextDecision().HasPrompt( "Select Growth" ).Choose( "Gain 2 Energy" );
		game.Spirit.NextDecision().HasPrompt( "Select Growth" ).HasOptions( "Place Presence(1)" ).ChooseFirst();
		game.Spirit.NextDecision().HasPrompt( "Select Presence to place" ); // engine is now waiting mid-action

		// Can't Save() here (mid-action) but the checkpoint is the clean state at the Place Presence menu.
		JsonObject checkpoint = game.LatestCheckpoint.ShouldNotBeNull();
		int energyAtMenu = game.Spirit.Energy; // Gain 2 Energy was done, Place Presence has cost nothing

		SoloGame resumed = Resume( checkpoint );

		resumed.Spirit.Energy.ShouldBe( energyAtMenu );
		Titles( await resumed.AdvanceAsync() ).ShouldBe( ["Place Presence(1)"] );
	}

}
