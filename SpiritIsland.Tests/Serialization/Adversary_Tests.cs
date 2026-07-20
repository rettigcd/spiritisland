namespace SpiritIsland.Tests.Serialization;

/// <summary>
/// Round-trips for AdversaryConfig/AdversaryRegistry - docs/GameSerialization-Roadmap.md section 9,
/// tier 2 (identity only). Level/escalation is static config fixed at construction, never reassigned
/// anywhere in the engine, so {Name, Level} fully determines identity - no wiring/state to round-trip
/// here, that's the harder tier 3 blocked on section 10 (hook action lists).
/// </summary>
public class Adversary_Tests {

	[Fact]
	public void AdversaryConfig_RoundTrips() {
		var original = new AdversaryConfig( "England", 3 );

		JsonArray json = original.ToJson();
		AdversaryConfig restored = AdversaryConfig.FromJson( json );

		restored.ShouldBe( original );
	}

	[Fact]
	public void AdversaryRegistry_ResolvesRealAdversary_ByNameAndLevel() {
		IAdversary original = new SpiritIsland.Basegame.England().Build( 3 );

		JsonArray json = original.Config.ToJson();
		IAdversary restored = AdversaryRegistry.Build( AdversaryConfig.FromJson( json ) );

		restored.Name.ShouldBe( "England" );
		restored.Level.ShouldBe( 3 );
		restored.ActiveLevels.Length.ShouldBe( original.ActiveLevels.Length );
	}

	[Fact]
	public void AdversaryRegistry_ResolvesNullAdversary() {
		JsonArray json = AdversaryConfig.NullAdversary.ToJson();

		IAdversary restored = AdversaryRegistry.Build( AdversaryConfig.FromJson( json ) );

		restored.Name.ShouldBe( "No Adversary" );
		restored.Level.ShouldBe( 0 );
	}

	static IEnumerable<IGameComponentProvider> AllProviders => [
		new SpiritIsland.Basegame.GameComponentProvider(),
		new SpiritIsland.BranchAndClaw.GameComponentProvider(),
		new SpiritIsland.FeatherAndFlame.GameComponentProvider(),
		new SpiritIsland.Horizons.GameComponentProvider(),
		new SpiritIsland.JaggedEarth.GameComponentProvider(),
		new SpiritIsland.NatureIncarnate.GameComponentProvider(),
	];

	[Fact]
	public void AdversaryRegistry_ResolvesEveryAdversaryAcrossEveryExpansion() {
		int checkedCount = 0;
		foreach( IGameComponentProvider provider in AllProviders ) {
			foreach( string adversaryName in provider.AdversaryNames ) {
				IAdversary original = provider.MakeAdversary( adversaryName )!.Build( 0 );

				IAdversary restored = AdversaryRegistry.Build( AdversaryConfig.FromJson( original.Config.ToJson() ) );

				restored.Name.ShouldBe( adversaryName );
				++checkedCount;
			}
		}
		checkedCount.ShouldBeGreaterThan( 5 ); // sanity check this actually exercised every expansion's adversaries
	}

	[Fact]
	public void ReplayingInit_ThenWipingTokens_PreservesEngineWiring_WithoutDoubleApplyingBoardMutations() {
		// Demonstrates the section-9 restore strategy: replay Adversary.Init/AdjustPlacedTokens on a
		// normally-constructed GameState (its board-mutation side effects are harmless, since they get
		// discarded next), then Tokens_ForIsland.FromJson makes the JSON snapshot the sole source of
		// truth. Sweden level 1 exercises both halves in one adversary: its escalation level wires an
		// Engine (gs.InvaderDeck.Explore.Engine = SwedenExplorer, internal to SpiritIsland.Basegame -
		// checked by type name), and level 1 itself mutates the board (AddIslandMod(SwedenHeavyMining())).
		var originalGs = new GameState( [ new TestSpirit() ], [ Boards.A ], 0 );
		IAdversary adversary = new SpiritIsland.Basegame.Sweden().Build( 1 );
		originalGs.Adversary = adversary;
		originalGs.InvaderDeck = adversary.InvaderDeckBuilder.Build( 0 );

		adversary.Init( originalGs );
		originalGs.Initialize();
		adversary.AdjustPlacedTokens( originalGs );

		JsonObject tokensJson = originalGs.Tokens.ToJson( new GameStateSerializationContext( originalGs ) );
		int originalModCount = originalGs.Tokens[originalGs.Island.Boards[0][2]]
			.ModsOfType<SpiritIsland.Basegame.SwedenHeavyMining>().Count();
		originalModCount.ShouldBe( 1 ); // sanity check the mod is really there, not vacuously 0

		// "Restore": build a fresh GameState the same way, replay the adversary's wiring/board-mutation
		// side effects on it (nothing else reconstructs Engine/event wiring), then let the JSON snapshot
		// override whatever tokens/island-mods that replay produced.
		var restoredGs = new GameState( [ new TestSpirit() ], [ Boards.A ], 0 );
		IAdversary restoredAdversary = AdversaryRegistry.Build( adversary.Config );
		restoredGs.Adversary = restoredAdversary;
		restoredGs.InvaderDeck = restoredAdversary.InvaderDeckBuilder.Build( 0 );

		restoredAdversary.Init( restoredGs );
		restoredGs.Initialize();
		restoredAdversary.AdjustPlacedTokens( restoredGs );

		Tokens_ForIsland.FromJson( restoredGs.Tokens, tokensJson, new GameStateSerializationContext( restoredGs ) );

		// Wiring survived the wipe - Tokens_ForIsland.FromJson never touches InvaderDeck.
		restoredGs.InvaderDeck.Explore.Engine.GetType().Name.ShouldBe( "SwedenExplorer" );

		// Board mutation wasn't double-applied - exactly the count captured in the snapshot, not 2x from
		// Init running a second time (once for "original", once for "restore") before the wipe discarded it.
		restoredGs.Tokens[restoredGs.Island.Boards[0][2]]
			.ModsOfType<SpiritIsland.Basegame.SwedenHeavyMining>().Count().ShouldBe( originalModCount );
	}

	/// <summary>
	/// Russia_Level6_PressureForFastProfitRavageEngine caches a direct reference (_token) to a
	/// RecordBlightAdded it also registers as an island mod. Replaying Init on a restored GameState (same
	/// strategy as the Sweden test above) reconstructs the engine and a fresh RecordBlightAdded together,
	/// but Tokens_ForIsland.FromJson then wipes/replaces island mods from the JSON snapshot - discarding
	/// that fresh instance and leaving the engine's _token pointing at neither the original nor the
	/// restored board. Since that orphaned instance is no longer a live island mod, it stops receiving
	/// HandleTokenAddedAsync callbacks entirely: PressureForFastProfit would then wrongly conclude every
	/// board received no Ravage Blight, forever, after any restore - see docs/GameSerialization-
	/// Roadmap.md's Adversary section.
	/// </summary>
	[Fact]
	public async Task RussiaLevel6_ResolveAfterTokensRestored_KeepsTokenWiredToBoardEvents() {
		var config = new GameConfiguration { Adversary = new AdversaryConfig( Russia.Name, 6 ), ShuffleNumber = 1 };

		GameState BuildRestoredGame( GameState original, JsonObject tokensJson, bool resolveAfterTokensRestored ) {
			GameState restoredGs = config.ConfigBoards( "A" ).ConfigSpirits( RiverSurges.Name ).BuildGame();
			var ctx = new GameStateSerializationContext( restoredGs );
			Tokens_ForIsland.FromJson( restoredGs.Tokens, tokensJson, ctx );
			if( resolveAfterTokensRestored )
				restoredGs.InvaderDeck.Ravage.Engine.ResolveAfterTokensRestored( ctx );
			return restoredGs;
		}

		async Task<Space> RavageASpaceWith3PlusExplorers( GameState gameState ) {
			Board board = gameState.Island.Boards[0];
			Space space = gameState.Tokens[board[5]];
			space.Given_ClearAll().Given_HasTokens( "1D@2,3E@1" );

			// a card that doesn't match this space by terrain - only Russia's "3+ Explorers" rule ravages it
			InvaderCard card = gameState.InvaderDeck.Explore.Cards.First( c => !c.MatchesCard( space ) );
			await gameState.InvaderDeck.Ravage.Engine.ActivateCard( card );
			return space;
		}

		var originalGs = config.ConfigBoards( "A" ).ConfigSpirits( RiverSurges.Name ).BuildGame();
		JsonObject tokensJson = originalGs.Tokens.ToJson( new GameStateSerializationContext( originalGs ) );

		// Without the fix: the space that received Blight this Ravage is wrongly also given the
		// Pressure-for-Fast-Profit bonus (1 Explorer + 1 Town), because the orphaned _token's
		// _receivedRavageBlight never got the board added to it.
		GameState buggyGs = BuildRestoredGame( originalGs, tokensJson, resolveAfterTokensRestored: false );
		Space buggySpace = await RavageASpaceWith3PlusExplorers( buggyGs );
		buggySpace.Sum( Human.Town ).ShouldBe( 1 );

		// With the fix: _token is re-pointed at the live island mod, so it correctly sees this board
		// received Blight and Pressure-for-Fast-Profit doesn't also fire for it.
		GameState fixedGs = BuildRestoredGame( originalGs, tokensJson, resolveAfterTokensRestored: true );
		Space fixedSpace = await RavageASpaceWith3PlusExplorers( fixedGs );
		fixedSpace.Sum( Human.Town ).ShouldBe( 0 );
	}

	static string BoardSummary( GameState gs ) => gs.Island.Boards[0].Spaces
		.Select( s => gs.Tokens[s].Summary )
		.Join( "|" );

	/// <summary>
	/// docs/GameSerialization-Roadmap.md's "Invader engine event/delegate fields" section: unlike
	/// Ravage.Engine (proven above), nothing yet proves GameState.RestoreFromJson preserves
	/// ExploreEngine.ExploredSpace/BuildEngine.BuildComplete-style `+=` subscriptions. Both are wired via
	/// stateless static method groups (France.ExploreTheFrontier/SlaveLaborBuildAsync close over nothing
	/// live), so replaying setup the normal way should reproduce them identically - this proves it by
	/// running a full Invader Phase on a restored instance and a never-restored control built the exact
	/// same way, and requiring the resulting board state to match exactly. If RestoreFromJson ever reset
	/// InvaderDeck.Explore/Build.Engine (it doesn't touch them today - see InvaderDeck.RestoreFromJson),
	/// this would diverge immediately since only the control would still have France's rules wired.
	///
	/// Ordering matters here in a way it doesn't elsewhere in this file: GameState's constructor
	/// unconditionally calls `ActionScope.Initialize(RootScope)`, rebinding the single ambient
	/// AsyncLocal every `ActionScope.Current` call in this process reads from. Building `restoredGs`
	/// *before* fully finishing with `controlGs` would silently redirect any `ActionScope.Current`-based
	/// code (most of InvaderPhase/Explore/Build) from `controlGs` onto `restoredGs` even though
	/// `controlGs` is still the explicit parameter - so `controlGs` is built, played, and snapshotted to
	/// a plain string *before* `restoredGs` is even constructed.
	/// </summary>
	[Fact]
	public async Task FranceLevel2_RestoreFromJson_PreservesExploreAndBuildEngineWiring() {
		// Level 1 wires Explore.Engine.ExploredSpace (Frontier Explorers); level 2 wires
		// Build.Engine.BuildComplete (Slave Labor) - both replay cumulatively at level 2.
		var config = new GameConfiguration { Adversary = new AdversaryConfig( France.Name, 2 ), ShuffleNumber = 1 };

		GameState controlGs = config.ConfigBoards( "A" ).ConfigSpirits( RiverSurges.Name ).BuildGame();
		JsonObject json = controlGs.ToJson( new GameStateSerializationContext( controlGs ) );
		await InvaderPhase.ActAsync( controlGs );
		string expectedSummary = BoardSummary( controlGs );

		GameState restoredGs = config.ConfigBoards( "A" ).ConfigSpirits( RiverSurges.Name ).BuildGame();
		restoredGs.RestoreFromJson( json, new GameStateSerializationContext( restoredGs ) );
		await InvaderPhase.ActAsync( restoredGs );

		BoardSummary( restoredGs ).ShouldBe( expectedSummary );
	}

	/// <summary>
	/// Same proof as the France test above, for BuildOnceOnSpace_Default.BuildUnitPicker and a
	/// Build.Engine type-swap: Habsburg Monarchy level 1 replaces Build.Engine with HabsurgBuilder
	/// (internal to SpiritIsland.JaggedEarth); level 2 then overrides BuildUnitPicker with the static
	/// DetermineWhatToBuild - both replay cumulatively at level 2, onto the *new* HabsurgBuilder's own
	/// OneSpacebuilder. Uses BuildEngine.TryToDo1Build(space) directly rather than a full Invader Phase -
	/// Habsburg's own Escalation (SeekPrimeTerritory_Escalation) asks the player to pick a land, a real
	/// decision a full phase risks hitting unresolved; TryToDo1Build exercises BuildUnitPicker/
	/// BuildComplete/the engine swap directly without going through Explore/card-matching at all.
	/// </summary>
	[Fact]
	public async Task HabsburgMonarchyLevel2_RestoreFromJson_PreservesBuildEngineSwapAndUnitPicker() {
		var config = new GameConfiguration { Adversary = new AdversaryConfig( HabsburgMonarchy.Name, 2 ), ShuffleNumber = 1 };

		static async Task<Space> BuildOnceOnASpace( GameState gs ) {
			Space space = gs.Tokens[ gs.Island.Boards[0][5] ];
			space.Given_ClearAll().Given_HasTokens( "1D@2" );
			await gs.InvaderDeck.Build.Engine.TryToDo1Build( space );
			return space;
		}

		GameState controlGs = config.ConfigBoards( "A" ).ConfigSpirits( RiverSurges.Name ).BuildGame();
		JsonObject json = controlGs.ToJson( new GameStateSerializationContext( controlGs ) );
		Space controlSpace = await BuildOnceOnASpace( controlGs );
		string expectedSummary = controlSpace.Summary;

		GameState restoredGs = config.ConfigBoards( "A" ).ConfigSpirits( RiverSurges.Name ).BuildGame();
		restoredGs.RestoreFromJson( json, new GameStateSerializationContext( restoredGs ) );

		// Engine swap itself survives (RestoreFromJson never touches InvaderDeck.Build.Engine).
		restoredGs.InvaderDeck.Build.Engine.GetType().Name.ShouldBe( "HabsurgBuilder" );

		Space restoredSpace = await BuildOnceOnASpace( restoredGs );

		restoredSpace.Summary.ShouldBe( expectedSummary );
	}

	/// <summary>
	/// Same proof again, for InvaderSlot.ActionComplete - an AsyncEvent&lt;GameState&gt; (not a plain C#
	/// event), populated via .Add(...) rather than +=. Habsburg Mining Expedition level 5 wires
	/// Build.ActionComplete to the static BuildThenUpgradeExplorer, which picks a land then asks the
	/// player which Explorer there to upgrade - a real decision, unlike the other two tests above, so
	/// this invokes ActionComplete directly (public on AsyncEvent&lt;T&gt;) rather than running a full
	/// Invader Phase, and resolves that one decision explicitly instead of risking an unresolved prompt
	/// hanging the test. NatureIncarnate isn't one of TestGames.GameBuilder's registered providers, so
	/// this constructs the GameState manually - the same recipe GameBuilder.BuildGame follows, mirroring
	/// ReplayingInit_ThenWipingTokens... above. Same build-then-fully-use-before-building-the-next-one
	/// ordering as the France test above, for the same ActionScope.Current/AsyncLocal reason.
	/// </summary>
	[Fact]
	public async Task HabsburgMiningExpeditionLevel5_RestoreFromJson_PreservesActionCompleteWiring() {
		static GameState BuildManually() {
			var gs = new GameState( [ new TestSpirit() ], [ Boards.A ], 0 );
			IAdversary adversary = new SpiritIsland.NatureIncarnate.HabsburgMiningExpedition().Build( 5 );
			gs.Adversary = adversary;
			gs.InvaderDeck = adversary.InvaderDeckBuilder.Build( 0 );
			// GameConfiguration.BuildGame always sets these; this manual recipe must too, since
			// GameState.ToJson asserts MajorCards/MinorCards non-null (docs/GameSerialization-
			// Roadmap.md's PowerCardDeck section - now closed, every real game sets both).
			gs.MajorCards = new PowerCardDeck( [], 0, PowerType.Major );
			gs.MinorCards = new PowerCardDeck( [], 0, PowerType.Minor );
			adversary.Init( gs );
			gs.Initialize();
			adversary.AdjustPlacedTokens( gs );
			return gs;
		}

		static async Task<Space> UpgradeTheOnlyExplorer( GameState gs ) {
			// Wipe every other Explorer setup already put on the board (e.g. via InitialExplore) so
			// this space is the *only* one with an Explorer - otherwise "OneLandPerBoard" itself asks
			// the player to pick among several qualifying lands, a second decision this test doesn't
			// need to also drive.
			gs.Given_InvadersDisappear();
			Space space = gs.Tokens[ gs.Island.Boards[0][5] ];
			space.Given_ClearAll().Given_HasTokens( "1E@1" );

			await gs.InvaderDeck.Build.ActionComplete.InvokeAsync( gs ).AwaitUser( u => {
				u.NextDecision.HasPrompt( "Select space to Upgrade explorer" ).Choose( "A5" );
				u.NextDecision.HasPrompt( "Select Explorer to Upgrade" ).Choose( "E@1 on A5" );
			} );
			return space;
		}

		GameState controlGs = BuildManually();
		JsonObject json = controlGs.ToJson( new GameStateSerializationContext( controlGs ) );
		Space controlSpace = await UpgradeTheOnlyExplorer( controlGs );
		string expectedSummary = controlSpace.Summary;

		GameState restoredGs = BuildManually();
		restoredGs.RestoreFromJson( json, new GameStateSerializationContext( restoredGs ) );
		Space restoredSpace = await UpgradeTheOnlyExplorer( restoredGs );

		restoredSpace.Summary.ShouldBe( expectedSummary );
	}

}
