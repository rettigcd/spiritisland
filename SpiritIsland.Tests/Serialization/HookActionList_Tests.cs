namespace SpiritIsland.Tests.Serialization;

/// <summary>
/// Round-trips for the "Free"/"Small"/"Small-Medium" hook-action-list entries and the generic
/// ActionList&lt;T&gt; JSON shape - docs/GameSerialization-Roadmap.md section 10. The remaining Medium/
/// Blocked entries in that section's table have no JSON path yet - the registries throw for anything
/// not registered, same fail-loud approach as every other registry in this project.
/// </summary>
public class HookActionList_Tests {

	static GameStateSerializationContext CtxFor( GameState gs ) => new( gs );

	[Fact]
	public void TimePassesActionRegistry_ResolvesTokens() {
		var gs = new SoloGameState();
		var ctx = CtxFor( gs );

		JsonArray json = TimePassesActionRegistry.Serialize( gs.Tokens, ctx );
		IRunWhenTimePasses restored = TimePassesActionRegistry.Deserialize( json, ctx );

		restored.ShouldBeSameAs( gs.Tokens );
	}

	[Fact]
	public void TimePassesActionRegistry_ResolvesSpirit() {
		var gs = new SoloGameState();
		var ctx = CtxFor( gs );

		JsonArray json = TimePassesActionRegistry.Serialize( gs.Spirit, ctx );
		IRunWhenTimePasses restored = TimePassesActionRegistry.Deserialize( json, ctx );

		restored.ShouldBeSameAs( gs.Spirit );
	}

	[Fact]
	public void TimePassesActionRegistry_ResolvesHealer_WithSkipSetsRestored() {
		var gs = new SoloGameState();
		var ctx = CtxFor( gs );
		Space space = gs.Tokens[gs.Board[3]];
		gs.Healer.SkipInvadersOn( space );
		gs.Healer.SkipDahanOn( space );

		JsonArray json = TimePassesActionRegistry.Serialize( gs.Healer, ctx );
		Healer restored = (Healer)TimePassesActionRegistry.Deserialize( json, ctx );

		restored.ShouldBeSameAs( gs.Healer ); // GameState owns exactly one Healer

		// _skipInvadersOn/_skipDahanOn are private - reflect to confirm the restored sets contain a
		// Space equal (by SpaceSpec.Label) to the one skipped before the round-trip.
		var invadersField = typeof( Healer ).GetField( "_skipInvadersOn", BindingFlags.NonPublic | BindingFlags.Instance )!;
		var dahanField = typeof( Healer ).GetField( "_skipDahanOn", BindingFlags.NonPublic | BindingFlags.Instance )!;
		( (HashSet<Space>)invadersField.GetValue( restored )! ).ShouldContain( gs.Tokens[gs.Board[3]] );
		( (HashSet<Space>)dahanField.GetValue( restored )! ).ShouldContain( gs.Tokens[gs.Board[3]] );
	}

	[Fact]
	public void ActionList_RoundTrips_FreeEntries_PreservingOrder() {
		var gs = new SoloGameState();
		var ctx = CtxFor( gs );

		var list = new TimePassesActionList();
		list.Add( gs.Healer );
		list.Add( gs.Tokens );
		list.Add( gs.Spirit );

		JsonArray json = list.ToJson( ctx, TimePassesActionRegistry.Serialize );

		var restored = new TimePassesActionList();
		restored.RestoreFromJson( json, ctx, TimePassesActionRegistry.Deserialize );

		restored.Actions.ShouldBe( [ gs.Healer, gs.Tokens, gs.Spirit ] ); // reference equality, in order
	}

	[Fact]
	public void GameState_RoundTrips_TimePassesActions_ViaWrapperMethods() {
		var gs = new SoloGameState();
		var ctx = CtxFor( gs );
		gs.AddTimePassesAction( gs.Spirit ); // GameState's own ctor already added Tokens/Healer

		JsonArray json = gs.TimePassesActionsToJson( ctx );
		gs.RestoreTimePassesActionsFromJson( json, ctx );

		// Restoring wholesale-replaces the list with an equivalent one - proven by round-tripping once
		// more and getting the identical JSON back, rather than reaching into the private field.
		gs.TimePassesActionsToJson( ctx ).ToJsonString().ShouldBe( json.ToJsonString() );
	}

	[Fact]
	public void TimePassesActionRegistry_ThrowsForUnknownType() {
		var gs = new SoloGameState();
		var ctx = CtxFor( gs );

		Should.Throw<NotSupportedException>( () => TimePassesActionRegistry.Serialize( new StubTimePassesAction(), ctx ) );
		Should.Throw<NotSupportedException>( () => TimePassesActionRegistry.Deserialize( new JsonArray( "Unknown" ), ctx ) );
	}

	class StubTimePassesAction : IRunWhenTimePasses {
		public bool RemoveAfterRun => false;
		public TimePassesOrder Order => TimePassesOrder.Normal;
		public Task TimePasses( GameState gameState ) => Task.CompletedTask;
	}

	[Fact]
	public void TimePassesActionRegistry_ResolvesReduceHealthByStrife() {
		var gs = new SoloGameState();
		var ctx = CtxFor( gs );
		var original = new ReduceHealthByStrife();

		JsonArray json = TimePassesActionRegistry.Serialize( original, ctx );
		IRunWhenTimePasses restored = TimePassesActionRegistry.Deserialize( json, ctx );

		restored.ShouldBeOfType<ReduceHealthByStrife>();
	}

	[Fact]
	public void TimePassesActionRegistry_ResolvesCommandBeasts_WithUsedFlag() {
		var gs = new SoloGameState();
		var ctx = CtxFor( gs );
		var original = new CommandBeasts( CommandBeasts.Stage2 );
		( (IHaveMemento)original ).Memento = true; // sets _used, same mechanism undo already relies on

		JsonArray json = TimePassesActionRegistry.Serialize( original, ctx );
		var restored = (CommandBeasts)TimePassesActionRegistry.Deserialize( json, ctx );

		restored.Title.ShouldBe( CommandBeasts.Stage2 );
		( (IRunWhenTimePasses)restored ).RemoveAfterRun.ShouldBeTrue(); // RemoveAfterRun => _used
	}

	[Fact]
	public void TimePassesActionRegistry_ResolvesSlowAndSilentDeath_ViaShroudSpiritInit() {
		// SlowAndSilentDeath is internal to SpiritIsland.JaggedEarth - exercised through the real
		// ShroudOfSilentMist.InitializeInternal path (which always adds one) rather than constructing
		// it directly, since the test project can't name an internal cross-assembly type.
		var spirit = new SpiritIsland.JaggedEarth.ShroudOfSilentMist();
		var gs = new SoloGameState( spirit );
		spirit.InitSpirit( gs.Board, gs );
		var ctx = CtxFor( gs );

		JsonArray json = gs.TimePassesActionsToJson( ctx );
		gs.RestoreTimePassesActionsFromJson( json, ctx );

		gs.TimePassesActionsToJson( ctx ).ToJsonString().ShouldBe( json.ToJsonString() );
	}

	[Fact]
	public void PreInvaderPhaseActionRegistry_ResolvesSlaveRebellion() {
		var gs = new SoloGameState();
		var ctx = CtxFor( gs );
		Type slaveRebellionType = typeof( SpiritIsland.BranchAndClaw.France ).Assembly.GetType( "SpiritIsland.BranchAndClaw.France+SlaveRebellion" )!;
		var original = (IRunBeforeInvaderPhase)Activator.CreateInstance( slaveRebellionType, nonPublic: true )!;

		JsonArray json = original.ToJson( ctx );
		IRunBeforeInvaderPhase restored = PreInvaderPhaseActionRegistry.Deserialize( json, ctx );

		restored.ShouldBeOfType( slaveRebellionType );
	}

	[Fact]
	public void PreInvaderPhaseActionRegistry_ResolvesLandStrippedBare() {
		var gs = new SoloGameState();
		var ctx = CtxFor( gs );
		Type landStrippedBareType = typeof( SpiritIsland.NatureIncarnate.HabsburgMiningExpedition ).Assembly.GetType( "SpiritIsland.NatureIncarnate.HabsburgMiningExpedition+LandStrippedBare" )!;
		var original = (IRunBeforeInvaderPhase)Activator.CreateInstance( landStrippedBareType, nonPublic: true )!;

		JsonArray json = original.ToJson( ctx );
		IRunBeforeInvaderPhase restored = PreInvaderPhaseActionRegistry.Deserialize( json, ctx );

		restored.ShouldBeOfType( landStrippedBareType );
	}

	[Fact]
	public void PreInvaderPhaseActionList_RoundTrips_PreservingOrder() {
		var gs = new SoloGameState();
		var ctx = CtxFor( gs );
		Type slaveRebellionType = typeof( SpiritIsland.BranchAndClaw.France ).Assembly.GetType( "SpiritIsland.BranchAndClaw.France+SlaveRebellion" )!;
		Type landStrippedBareType = typeof( SpiritIsland.NatureIncarnate.HabsburgMiningExpedition ).Assembly.GetType( "SpiritIsland.NatureIncarnate.HabsburgMiningExpedition+LandStrippedBare" )!;

		var list = new PreInvaderPhaseActionList();
		list.Add( (IRunBeforeInvaderPhase)Activator.CreateInstance( slaveRebellionType, nonPublic: true )! );
		list.Add( (IRunBeforeInvaderPhase)Activator.CreateInstance( landStrippedBareType, nonPublic: true )! );

		JsonArray json = list.ToJson( ctx, ( action, c ) => action.ToJson( c ) );

		var restored = new PreInvaderPhaseActionList();
		restored.RestoreFromJson( json, ctx, PreInvaderPhaseActionRegistry.Deserialize );

		restored.Actions.Select( a => a.GetType() ).ShouldBe( [ slaveRebellionType, landStrippedBareType ] );
	}

	[Fact]
	public void GameState_RoundTrips_PreInvaderPhaseActions_ViaWrapperMethods_IncludingSlowDissolutionOfWillMod() {
		// Also proves SlowDissolutionOfWillMod (already registry-ready per section 7) is now wired into
		// an actual list round-trip, not just callable in isolation.
		var gs = new SoloGameState();
		var ctx = CtxFor( gs );
		Type modType = typeof( SpiritIsland.NatureIncarnate.SlowDissolutionOfWill ).Assembly.GetType( "SpiritIsland.NatureIncarnate.SlowDissolutionOfWillMod" )!;
		gs.AddPreInvaderPhaseAction( (IRunBeforeInvaderPhase)Activator.CreateInstance( modType, nonPublic: true )! );

		JsonArray json = gs.PreInvaderPhaseActionsToJson( ctx );
		gs.RestorePreInvaderPhaseActionsFromJson( json, ctx );

		gs.PreInvaderPhaseActionsToJson( ctx ).ToJsonString().ShouldBe( json.ToJsonString() );
	}

	// -- Medium-tier identity-resolution entries (section 10) --

	[Theory]
	[InlineData( typeof( SpiritIsland.Basegame.DownwardSpiral ) )]
	[InlineData( typeof( SpiritIsland.Basegame.MemoryFadesToDust ) )]
	[InlineData( typeof( SpiritIsland.JaggedEarth.PowerCorrodesTheSpirit ) )]
	[InlineData( typeof( SpiritIsland.JaggedEarth.UntendedLandCrumbles ) )]
	[InlineData( typeof( SpiritIsland.NatureIncarnate.AttenuatedEssence ) )]
	[InlineData( typeof( SpiritIsland.NatureIncarnate.BlightCorrodesTheSpirit ) )]
	[InlineData( typeof( SpiritIsland.NatureIncarnate.TheBorderOfLifeAndDeath ) )]
	public void PreInvaderPhaseActionRegistry_ResolvesSelfRegisteringBlightCards_ToLiveGameStateBlightCard( Type blightCardType ) {
		var gs = new SoloGameState();
		gs.BlightCard = (BlightCard)Activator.CreateInstance( blightCardType )!;
		var ctx = CtxFor( gs );
		var original = (IRunBeforeInvaderPhase)gs.BlightCard;

		JsonArray json = original.ToJson( ctx );
		IRunBeforeInvaderPhase restored = PreInvaderPhaseActionRegistry.Deserialize( json, ctx );

		restored.ShouldBeSameAs( gs.BlightCard ); // not a fresh BlightCardRegistry construction
	}

	[Fact]
	public void TimePassesActionRegistry_ResolvesHighImmegrationSlot_ToLiveActiveSlotsInstance() {
		var gs = new SoloGameState();
		var slot = new SpiritIsland.Basegame.England.HighImmegrationSlot( level: 3 );
		gs.InvaderDeck.ActiveSlots.Insert( 0, slot ); // as England's level-3 InitFunc does
		var ctx = CtxFor( gs );
		var original = (IRunWhenTimePasses)slot;

		JsonArray json = TimePassesActionRegistry.Serialize( original, ctx );
		IRunWhenTimePasses restored = TimePassesActionRegistry.Deserialize( json, ctx );

		restored.ShouldBeSameAs( slot ); // not a fresh InvaderSlotRegistry construction
	}

	[Fact]
	public void Reclaim1InsteadOfDiscard_RoundTrips_SpiritAndPurchasedSnapshot() {
		var gs = new SoloGameState();
		PowerCard card = gs.Spirit.Hand.Single();
		gs.Spirit.Hand.Remove( card );
		gs.Spirit.InPlay.Add( card ); // snapshot source - Reclaim1InsteadOfDiscard captures InPlay at construction
		var ctx = CtxFor( gs );
		var original = new Reclaim1InsteadOfDiscard( gs.Spirit );

		JsonArray json = ( (ISerializableTimePassesAction)original ).ToJson( ctx );
		var restored = (Reclaim1InsteadOfDiscard)TimePassesActionRegistry.Deserialize( json, ctx );

		// spirit/purchased are private - reflect to confirm both resolved correctly.
		var spiritField = typeof( Reclaim1InsteadOfDiscard ).GetField( "spirit", BindingFlags.NonPublic | BindingFlags.Instance )!;
		var purchasedField = typeof( Reclaim1InsteadOfDiscard ).GetField( "_purchased", BindingFlags.NonPublic | BindingFlags.Instance )!;
		spiritField.GetValue( restored ).ShouldBeSameAs( gs.Spirit );
		// Compare by Title, not reference: TestSpirit's card is built directly via PowerCard.ForDecorated
		// rather than seeded into PowerCardRegistry, so its title can collide with an unrelated real
		// spirit's card of the same name (same convention Spirit_Tests.RoundTrips_HandInPlayDiscardPile uses).
		( (PowerCard[])purchasedField.GetValue( restored )! ).Select( c => c.Title ).ShouldBe( [ card.Title ] );
	}

	[Fact]
	public void PostInvaderPhaseActionRegistry_ResolvesTriggerAfterNoRavageOrBuild_ToLiveBoardInstance() {
		var gs = new SoloGameState();
		var space = gs.Board[2];
		var ctx = CtxFor( gs );
		var targetCtx = gs.Spirit.Target( space );
		var original = new SpiritIsland.NatureIncarnate.TriggerAfterNoRavageOrBuild( gs.Spirit, targetCtx, Token.Beast );
		gs.Tokens[space].Init( original, 1 ); // simulate it already being placed on the board by Tokens_ForIsland.FromJson

		JsonArray json = ( (IRunAfterInvaderPhase)original ).ToJson( ctx );
		IRunAfterInvaderPhase restored = PostInvaderPhaseActionRegistry.Deserialize( json, ctx );

		restored.ShouldBeSameAs( original ); // not a fresh SpaceEntitySerialization construction
	}

	[Fact]
	public void GameState_RoundTrips_PostInvaderPhaseActions_ViaWrapperMethods() {
		var gs = new SoloGameState();
		var space = gs.Board[2];
		var ctx = CtxFor( gs );
		var targetCtx = gs.Spirit.Target( space );
		var trigger = new SpiritIsland.NatureIncarnate.TriggerAfterNoRavageOrBuild( gs.Spirit, targetCtx, Token.Beast );
		gs.Tokens[space].Init( trigger, 1 );
		gs.AddPostInvaderPhase( trigger );

		JsonArray json = gs.PostInvaderPhaseActionsToJson( ctx );
		gs.RestorePostInvaderPhaseActionsFromJson( json, ctx );

		gs.PostInvaderPhaseActionsToJson( ctx ).ToJsonString().ShouldBe( json.ToJsonString() );
	}

	// -- Tier-1 "closure -> named type" conversions (docs/GameSerialization-Roadmap.md section 10) --
	// Each of these is internal to its own expansion assembly, so - same as SlaveRebellion/
	// LandStrippedBare above - it's constructed via reflection rather than a public API.

	static object CreateInternal( Type type, params object[] args )
		=> Activator.CreateInstance( type, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, null, args, null )!;

	[Fact]
	public async Task TimePassesActionRegistry_ResolvesFlamesFury_RemoveBonusDamage() {
		var gs = new SoloGameState();
		var ctx = CtxFor( gs );
		Type type = typeof( SpiritIsland.FeatherAndFlame.FlamesFury ).Assembly.GetType( "SpiritIsland.FeatherAndFlame.FlamesFury+RemoveBonusDamage" )!;
		var original = (IRunWhenTimePasses)CreateInternal( type, gs.Spirit );

		JsonArray json = TimePassesActionRegistry.Serialize( original, ctx );
		var restored = (IRunWhenTimePasses)TimePassesActionRegistry.Deserialize( json, ctx );

		gs.Spirit.BonusDamage = 3;
		await restored.TimePasses( gs );
		gs.Spirit.BonusDamage.ShouldBe( 2 );
	}

	[Fact]
	public async Task TimePassesActionRegistry_ResolvesAbsoluteStasis_RestoreSpaceExistence() {
		var gs = new SoloGameState();
		var space = gs.Board[2];
		var ctx = CtxFor( gs );
		Type type = typeof( SpiritIsland.JaggedEarth.AbsoluteStasis ).Assembly.GetType( "SpiritIsland.JaggedEarth.AbsoluteStasis+RestoreSpaceExistence" )!;
		var original = (IRunWhenTimePasses)CreateInternal( type, space );

		JsonArray json = TimePassesActionRegistry.Serialize( original, ctx );
		var restored = (IRunWhenTimePasses)TimePassesActionRegistry.Deserialize( json, ctx );

		space.DoesExists = false;
		await restored.TimePasses( gs );
		space.DoesExists.ShouldBeTrue();
	}

	[Fact]
	public async Task TimePassesActionRegistry_ResolvesEntwinedPower_RestoreTargetingSourceStrategy() {
		var spiritA = new TestSpirit();
		var spiritB = new TestSpirit();
		var gs = new GameState( [ spiritA, spiritB ], [ Boards.A ], 0 );
		var ctx = CtxFor( gs );

		ITargetingSourceStrategy selfOrig = spiritA.TargetingSourceStrategy;
		ITargetingSourceStrategy otherOrig = spiritB.TargetingSourceStrategy;

		Type restoreType = typeof( SpiritIsland.Basegame.EntwinedPower ).Assembly.GetType( "SpiritIsland.Basegame.EntwinedPower+RestoreTargetingSourceStrategy" )!;
		var original = (IRunWhenTimePasses)CreateInternal( restoreType, spiritA, selfOrig, spiritB, otherOrig );

		JsonArray json = TimePassesActionRegistry.Serialize( original, ctx );
		var restored = (IRunWhenTimePasses)TimePassesActionRegistry.Deserialize( json, ctx );

		// Simulate Entwined Power's override actually being active on both spirits when this fires -
		// same array-covariance trap noted in TargetingSourceStrategyAndRangeCalc_Tests, box the
		// Spirit[] as the single ctor arg.
		Type entwinedType = typeof( SpiritIsland.Basegame.EntwinedPower ).Assembly.GetType( "SpiritIsland.Basegame.EntwinedPower+EntwinedPresenceSource" )!;
		Activator.CreateInstance( entwinedType, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, null, [ new Spirit[] { spiritA, spiritB } ], null );
		spiritA.TargetingSourceStrategy.ShouldBeOfType( entwinedType );
		spiritB.TargetingSourceStrategy.ShouldBeOfType( entwinedType );

		await restored.TimePasses( gs );

		spiritA.TargetingSourceStrategy.ShouldBeOfType<DefaultPowerSourceStrategy>();
		spiritB.TargetingSourceStrategy.ShouldBeOfType<DefaultPowerSourceStrategy>();
	}

	[Fact]
	public async Task TimePassesActionRegistry_ResolvesUnlockTheGatesOfDeepestPower_ForgetCardAtEndOfTurn() {
		// Use a real spirit (not TestSpirit) so its Hand card is actually seeded into PowerCardRegistry.
		// Even then, provider.MakeSpirit here builds a *fresh* instance separate from whatever earlier
		// MakeSpirit call originally seeded the registry - resolve through PowerCardRegistry up front so
		// spirit.InPlay holds the exact reference ForgetCardAtEndOfTurn's reader will hand back; Forget.
		// ThisCard removes by reference (List.Remove, PowerCard has no Equals override).
		var provider = new SpiritIsland.Basegame.GameComponentProvider();
		Spirit spirit = provider.MakeSpirit( SpiritIsland.Basegame.Shadows.Name )!;
		var gs = new SoloGameState( spirit ); // ctx.IndexOf(spirit) needs it in gs.Spirits
		var ctx = CtxFor( gs );
		PowerCard card = PowerCardRegistry.Deserialize( spirit.Hand.First().ToJson() );
		spirit.Hand.Clear();
		spirit.InPlay.Add( card );
		Type type = typeof( SpiritIsland.BranchAndClaw.UnlockTheGatesOfDeepestPower ).Assembly.GetType( "SpiritIsland.BranchAndClaw.UnlockTheGatesOfDeepestPower+ForgetCardAtEndOfTurn" )!;
		var original = (IRunWhenTimePasses)CreateInternal( type, spirit, card );

		JsonArray json = TimePassesActionRegistry.Serialize( original, ctx );
		var restored = (IRunWhenTimePasses)TimePassesActionRegistry.Deserialize( json, ctx );

		await restored.TimePasses( gs );
		spirit.InPlay.ShouldNotContain( card );
	}

	[Fact]
	public async Task TimePassesActionRegistry_ResolvesWeaveTogetherTheFabricOfPlace_UnweaveSpaces() {
		// UnweaveSpaces replaces the old TimePassesAction.Once(...) closure (docs/GameSerialization-
		// Roadmap.md's hook action lists section) - the interesting identity-resolution case here is that
		// spaceSpec/otherSpec are *detached* from their Board while the weave is active, so they only
		// resolve back via the still-attached joined space's MultiSpaceSpec.OrigSpaces, not directly by
		// label. One Explorer is left on the joined space so DistributeVisibleTokens (the "divide pieces
		// as you wish" step) has something to ask about - declining it (Done) is enough to prove the rest
		// of the sequence (reconnecting spaceSpec/otherSpec, moving the token back) runs correctly; that
		// step's own distribution logic is pre-existing game behavior, not something this conversion
		// needs to re-verify.
		var gs = new SoloGameState( new LureOfTheDeepWilderness(), Boards.B );
		var ctx = CtxFor( gs );
		gs.Tokens[gs.Board[2]].Given_ClearAll().Given_HasTokens( "1E@1" );
		gs.Tokens[gs.Board[4]].Given_ClearAll();

		await WeaveTogetherTheFabricOfPlace.ActAsync( gs.Spirit.Target( gs.Board[2] ) ).AwaitUser( user => {
			user.NextDecision.HasPrompt( "Join B2 to" ).Choose( "B4" );
		} );

		Type type = typeof( SpiritIsland.JaggedEarth.WeaveTogetherTheFabricOfPlace ).Assembly.GetType( "SpiritIsland.JaggedEarth.WeaveTogetherTheFabricOfPlace+UnweaveSpaces" )!;
		var original = TimePassesActions( gs ).Single( a => a.GetType() == type );

		JsonArray json = TimePassesActionRegistry.Serialize( original, ctx );
		var restored = (IRunWhenTimePasses)TimePassesActionRegistry.Deserialize( json, ctx );

		gs.Spaces_Unfiltered.Any( s => s.Label == "B2:B4" ).ShouldBeTrue(); // sanity - still joined
		gs.Spaces_Unfiltered.Any( s => s.Label == "B2" ).ShouldBeFalse();
		await restored.TimePasses( gs ).AwaitUser( user => {
			user.NextDecision.HasPrompt( "Distribute tokens to un-woven B4 up to (1)" ).Choose( "Done" );
		} );

		gs.Spaces_Unfiltered.Any( s => s.Label == "B2:B4" ).ShouldBeFalse();
		gs.Spaces_Unfiltered.Any( s => s.Label == "B2" ).ShouldBeTrue();
		gs.Spaces_Unfiltered.Any( s => s.Label == "B4" ).ShouldBeTrue();
		// Look up by label, not gs.Board[2] - Board.AddSpace re-attaches via _spaces.Union(...), which
		// appends rather than restoring the original array position, so B2 isn't necessarily back at
		// index 2 after a weave/unweave round trip.
		gs.Spaces_Unfiltered.Single( s => s.Label == "B2" ).Sum( Human.Explorer ).ShouldBe( 1 ); // declined to move it - stays put
	}

	[Fact]
	public async Task TimePassesActionRegistry_ResolvesPourTimeSideways_ResetInvaderActionCounts() {
		var gs = new SoloGameState();
		var ctx = CtxFor( gs );
		Board board = gs.Board;
		Type type = typeof( SpiritIsland.JaggedEarth.AbsoluteStasis ).Assembly.GetType( "SpiritIsland.JaggedEarth.PourTimeSideways+ResetInvaderActionCounts" )!;
		// Array covariance means passing a Board[] straight to a `params object[]` helper gets swallowed
		// as the args array itself rather than becoming args[0] - box it in an explicit object[] first.
		object[] ctorArgs = [ new Board[] { board } ];
		var original = (IRunWhenTimePasses)CreateInternal( type, ctorArgs );

		JsonArray json = TimePassesActionRegistry.Serialize( original, ctx );
		var restored = (IRunWhenTimePasses)TimePassesActionRegistry.Deserialize( json, ctx );

		board.InvaderActionCount = 5;
		await restored.TimePasses( gs );
		board.InvaderActionCount.ShouldBe( 1 );
	}

	// -- GameCmd.AtTheStartOfNextRound (Medium tier, docs/GameSerialization-Roadmap.md section 10) --
	// Both current callers build a fully static, parameterless command graph, so NextRoundCommand only
	// needs to round-trip which named command it is (via NextRoundCommandRegistry), not any captured
	// state - proven here by actually running the restored entry and checking its real effect landed.

	[Fact]
	public async Task GameState_RoundTrips_TimePassesActions_IncludingAllThingsWeakensNextRoundCommand() {
		var gs = new SoloGameState();
		var ctx = CtxFor( gs );
		var card = new SpiritIsland.JaggedEarth.AllThingsWeaken();
		await card.Immediately.ActAsync( gs ); // schedules the NextRoundCommand, same as Side1Depleted would

		JsonArray json = gs.TimePassesActionsToJson( ctx );
		gs.RestoreTimePassesActionsFromJson( json, ctx );
		gs.TimePassesActionsToJson( ctx ).ToJsonString().ShouldBe( json.ToJsonString() );

		await gs.RunTimePassesActions();

		var space = gs.Tokens[gs.Board[2]];
		space.ModsOfType<SpiritIsland.JaggedEarth.AllThingsWeaken.LandDamageBoost>().ShouldNotBeEmpty();
		space.ModsOfType<SpiritIsland.JaggedEarth.DestroyerOfBeastsAndPresence>().ShouldNotBeEmpty();
	}

	[Fact]
	public async Task GameState_RoundTrips_TimePassesActions_IncludingIntensifyingExploitationsNextRoundCommand() {
		var gs = new SoloGameState();
		var ctx = CtxFor( gs );
		var card = new SpiritIsland.NatureIncarnate.IntensifyingExploitation();
		await card.Immediately.ActAsync( gs );

		JsonArray json = gs.TimePassesActionsToJson( ctx );
		gs.RestoreTimePassesActionsFromJson( json, ctx );
		gs.TimePassesActionsToJson( ctx ).ToJsonString().ShouldBe( json.ToJsonString() );

		await gs.RunTimePassesActions();

		gs.Tokens[gs.Board[2]].ModsOfType<SpiritIsland.NatureIncarnate.IntensifyingExploitation.ExtraDamage>().ShouldNotBeEmpty();
	}

	[Fact]
	public void NextRoundCommandRegistry_ThrowsForUnknownTag() {
		Should.Throw<KeyNotFoundException>( () => NextRoundCommandRegistry.Get( "NoSuchTag" ) );
	}

	[Fact]
	public void GameState_RoundTrips_ReminderCards_ResolvingCommandBeasts_ToLiveTimePassesActionsInstance() {
		// Mirrors what CommandBeasts.OnCardRevealed does: added to both _timePassesActions and
		// ReminderCards together. RestoreReminderCardsFromJson must run after
		// RestoreTimePassesActionsFromJson so it resolves against the already-restored list.
		var gs = new SoloGameState();
		var ctx = CtxFor( gs );
		var commandBeasts = new CommandBeasts( CommandBeasts.Stage3 );
		gs.AddTimePassesAction( commandBeasts );
		gs.ReminderCards.Add( commandBeasts );

		JsonArray timePassesJson = gs.TimePassesActionsToJson( ctx );
		JsonArray reminderCardsJson = gs.ReminderCardsToJson( ctx );

		var target = new SoloGameState();
		var targetCtx = CtxFor( target );
		target.RestoreTimePassesActionsFromJson( timePassesJson, targetCtx );
		target.RestoreReminderCardsFromJson( reminderCardsJson, targetCtx );

		var restoredCommandBeasts = target.ReminderCards.Single();
		restoredCommandBeasts.ShouldBeOfType<CommandBeasts>();
		// Not a fresh, disconnected CommandBeasts - the same instance _timePassesActions restored, so
		// ActivateAsync's gs.ReminderCards.Remove(this) will actually find and remove it later.
		restoredCommandBeasts.ShouldBeSameAs( TimePassesActions( target ).OfType<CommandBeasts>().Single() );
	}

	static IReadOnlyList<IRunWhenTimePasses> TimePassesActions( GameState gs ) => ( (ActionList<IRunWhenTimePasses>)
		typeof( GameState ).GetField( "_timePassesActions", BindingFlags.NonPublic | BindingFlags.Instance )!.GetValue( gs )! ).Actions;

	[Fact]
	public void GameState_ReminderCardsToJson_ThrowsForUnsupportedEntry() {
		var gs = new SoloGameState();
		var ctx = CtxFor( gs );
		gs.ReminderCards.Add( new object() );

		Should.Throw<NotSupportedException>( () => gs.ReminderCardsToJson( ctx ) );
	}

}
