#nullable enable
namespace SpiritIsland.Tests.Serialization;

/// <summary>
/// Round-trips for Spirit.ToJson/RestoreFromJson and SpiritPresence.ToJson/RestoreFromJson -
/// docs/GameSerialization-Roadmap.md section 2. RestoreFromJson always overlays onto a *freshly
/// reconstructed* Spirit of the same concrete type/config as the original (mirroring "construct
/// through the normal path, then overwrite fields" from Fear/Island/BlightCard) - GrowthTrack,
/// InnatePowers, and Mods are assumed identical from reconstruction alone and aren't part of the
/// JSON at all. See the doc comment on Spirit.ToJson for the full list of what's deliberately not
/// captured and the known gaps (non-default TargetingSourceStrategy/PowerRangeCalc, non-Card/Innate/
/// GrowthAction action factories, CustomMementoValue).
/// </summary>
public class Spirit_Tests {

	static GameStateSerializationContext CtxFor( GameState gs ) => new( gs );

	[Fact]
	public void RoundTrips_BasicFields() {
		var gs = new SoloGameState();
		var ctx = CtxFor( gs );

		var original = new TestSpirit();
		original.Energy = 7;
		original.TempCardPlayBoost = 2;
		original.BonusDamage = 3;
		original.Elements[Element.Fire] = 4;
		original.Elements[Element.Moon] = 1;

		JsonObject json = original.ToJson( ctx );

		var target = new TestSpirit();
		target.RestoreFromJson( json, ctx );

		target.Energy.ShouldBe( 7 );
		target.TempCardPlayBoost.ShouldBe( 2 );
		target.BonusDamage.ShouldBe( 3 );
		target.Elements[Element.Fire].ShouldBe( 4 );
		target.Elements[Element.Moon].ShouldBe( 1 );
	}

	[Fact]
	public void RoundTrips_HandInPlayDiscardPile() {
		var gs = new SoloGameState();
		var ctx = CtxFor( gs );

		var original = new TestSpirit();
		PowerCard card = original.Hand.Single();
		original.Hand.Remove( card );
		original.InPlay.Add( card );

		JsonObject json = original.ToJson( ctx );

		var target = new TestSpirit();
		target.RestoreFromJson( json, ctx );

		target.Hand.ShouldBeEmpty();
		target.InPlay.Select( c => c.Title ).ShouldBe( [ card.Title ] );
		target.DiscardPile.ShouldBeEmpty();
	}

	[Fact]
	public async Task RoundTrips_PresenceRevealedTracksDestroyedAndIncarnaEmpowered() {
		var gs = new SoloGameState();
		var ctx = CtxFor( gs );

		var original = new TestSpirit();
		await original.Presence.Energy.RevealAsync( original.Presence.Energy.Slots.ElementAt( 1 ) ); // 1 -> 2 revealed
		original.Presence.Destroyed.Count = 2;
		original.Presence.Incarna.Empowered = true;

		JsonObject json = original.ToJson( ctx );

		var target = new TestSpirit();
		target.RestoreFromJson( json, ctx );

		target.Presence.Energy.Revealed.Count().ShouldBe( 2 );
		target.Presence.CardPlays.Revealed.Count().ShouldBe( 1 ); // untouched, still default
		target.Presence.Destroyed.Count.ShouldBe( 2 );
		target.Presence.Incarna.Empowered.ShouldBeTrue();
	}

	/// <summary>
	/// CompoundPresenceTrack (Starlight Seeks Its Form's Energy/CardPlays, 3 parts each) used to throw in
	/// SpiritPresence.ToJson/RestoreFromJson - docs/GameSerialization-Roadmap.md's Spirit-core-state
	/// section. Reveals part0's 2nd slot and part2's 1st slot specifically (Energy2/Track_Gain1Energy -
	/// neither has an OnRevealAsync decision, unlike the "NewGrowth" slots elsewhere on this track, which
	/// present a real Growth-choice prompt when revealed), leaving the 1-slot middle part untouched.
	/// Checking RevealOptions.Count() afterward (not just Revealed.Count()) proves restore tracked *which
	/// part* revealed, not just a combined total - a "just serialize the total" bug that dumped all 3
	/// reveals into one part would fully exhaust that part's own RevealOptions entry, changing the count.
	/// </summary>
	[Fact]
	public async Task RoundTrips_CompoundPresenceTrack_PerPartRevealedCounts() {
		var gs = new SoloGameState();
		var ctx = CtxFor( gs );

		var original = new SpiritIsland.JaggedEarth.StarlightSeeksItsForm();
		// parts sized 4/1/2, starting revealed 1/0/0 - RevealOptions is [part0, part1, part2] in order.
		Track part0Slot = original.Presence.Energy.RevealOptions.ElementAt( 0 );
		Track part2Slot = original.Presence.Energy.RevealOptions.ElementAt( 2 );
		await original.Presence.Energy.RevealAsync( part0Slot );
		await original.Presence.Energy.RevealAsync( part2Slot );

		JsonObject json = original.ToJson( ctx );

		var target = new SpiritIsland.JaggedEarth.StarlightSeeksItsForm();
		target.RestoreFromJson( json, ctx );

		target.Presence.Energy.Revealed.Count().ShouldBe( 3 ); // 2 (part0) + 0 (part1) + 1 (part2)
		target.Presence.Energy.RevealOptions.Count().ShouldBe( 3 ); // none of the 3 parts got fully exhausted
		target.Presence.CardPlays.Revealed.Count().ShouldBe( 1 ); // untouched, still default
	}

	/// <summary>
	/// FinderTrack (Finder of Paths Unseen's Energy/CardPlays - a linked-slot graph, not a linear track)
	/// used to throw in SpiritPresence.ToJson/RestoreFromJson - same section as above. Revealing one more
	/// slot beyond the constructor's initial Energy0/Card1 reveals and comparing RevealOptions/
	/// ReturnableOptions counts (not just Revealed.Count()) proves the specific slot - and its cascading
	/// effect on linked neighbors - round-tripped correctly, not just an aggregate count.
	/// </summary>
	[Fact]
	public async Task RoundTrips_FinderTrack_PerSlotStates() {
		var gs = new SoloGameState();
		var ctx = CtxFor( gs );

		var original = new SpiritIsland.FeatherAndFlame.FinderOfPathsUnseen();
		await original.Presence.Energy.RevealAsync( original.Presence.Energy.RevealOptions.First() );

		JsonObject json = original.ToJson( ctx );

		var target = new SpiritIsland.FeatherAndFlame.FinderOfPathsUnseen();
		target.RestoreFromJson( json, ctx );

		target.Presence.Energy.Revealed.Count().ShouldBe( original.Presence.Energy.Revealed.Count() );
		target.Presence.Energy.RevealOptions.Count().ShouldBe( original.Presence.Energy.RevealOptions.Count() );
		target.Presence.Energy.ReturnableOptions.Count().ShouldBe( original.Presence.Energy.ReturnableOptions.Count() );
		target.Presence.CardPlays.Revealed.Count().ShouldBe( 1 ); // untouched, still just the initial Card1
	}

	[Fact]
	public void RoundTrips_DefaultTargetingSourceStrategyAndPowerRangeCalc() {
		var gs = new SoloGameState();
		var ctx = CtxFor( gs );

		var original = new TestSpirit(); // TargetingSourceStrategy/PowerRangeCalc are the defaults

		JsonObject json = original.ToJson( ctx );

		var target = new TestSpirit();
		target.TargetingSourceStrategy.ShouldNotBeSameAs( original.TargetingSourceStrategy ); // sanity: distinct instances pre-restore
		target.RestoreFromJson( json, ctx );

		target.TargetingSourceStrategy.ShouldBeOfType<DefaultPowerSourceStrategy>();
		target.PowerRangeCalc.ShouldBeSameAs( DefaultRangeCalculator.Singleton );
	}

	[Fact]
	public void RoundTrips_NonDefaultPowerRangeCalc_RangeExtender() {
		// docs/GameSerialization-Roadmap.md section 2's "known gap" - an active temporary override
		// (not just the default singleton) used to throw here. RangeExtender is public, so this proves
		// the fix at the Spirit.ToJson/RestoreFromJson level directly, no reflection needed.
		var gs = new SoloGameState();
		var ctx = CtxFor( gs );

		var original = new TestSpirit { PowerRangeCalc = new RangeExtender( 2, DefaultRangeCalculator.Singleton ) };

		JsonObject json = original.ToJson( ctx );

		var target = new TestSpirit();
		target.RestoreFromJson( json, ctx );

		var restored = target.PowerRangeCalc.ShouldBeOfType<RangeExtender>();
		restored.Previous.ShouldBeSameAs( DefaultRangeCalculator.Singleton );
	}

	[Fact]
	public void RoundTrips_NonDefaultTargetingSourceStrategy_IncludeSerpentsIncarna() {
		// Same gap, ITargetingSourceStrategy side. IncludeSerpentsIncarna is internal (FeatherAndFlame),
		// so construct it via reflection - same convention as HookActionList_Tests's internal types.
		var gs = new SoloGameState();
		var ctx = CtxFor( gs );

		Type type = typeof( SpiritIsland.FeatherAndFlame.FlamesFury ).Assembly.GetType( "SpiritIsland.FeatherAndFlame.LocusOfTheSerpentsRegard+IncludeSerpentsIncarna" )!;
		var strategy = (ITargetingSourceStrategy)Activator.CreateInstance(
			type, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, null, [ new DefaultPowerSourceStrategy(), gs.Spirit ], null
		)!;
		var original = new TestSpirit { TargetingSourceStrategy = strategy };

		JsonObject json = original.ToJson( ctx );

		var target = new TestSpirit();
		target.RestoreFromJson( json, ctx );

		target.TargetingSourceStrategy.ShouldBeOfType( type );
	}

	[Fact]
	public void RoundTrips_AvailableAndUsedActions_PowerCardAndGrowthAction() {
		var gs = new SoloGameState();
		var ctx = CtxFor( gs );

		var original = new TestSpirit();
		PowerCard card = original.Hand.Single();
		IActionFactory growthAction = original.GrowthTrack.Groups[0].GrowthActionFactories[0];

		original.AddActionFactory( card );        // available
		original.AddActionFactory( growthAction ); // available
		original.MarkAsResolved( card );           // -> used

		JsonObject json = original.ToJson( ctx );

		var target = new TestSpirit();
		target.RestoreFromJson( json, ctx );

		target.UsedActions.Select( a => a.Title ).ShouldBe( [ card.Title ] );
		target.AllActions.Select( a => a.Title ).ShouldContain( growthAction.Title );
		target.AllActions.OfType<PowerCard>().ShouldBeEmpty(); // the card was used, not left available
	}

	[Fact]
	public void RoundTrips_AvailableAction_FastSlowAction_WrappingPlayCardForCost() {
		var gs = new SoloGameState();
		var ctx = CtxFor( gs );

		var original = new TestSpirit();
		original.AddActionFactory( new FastSlowAction( new PlayCardForCost( Present.Done, tax: 1 ) ) );

		JsonObject json = original.ToJson( ctx );

		var target = new TestSpirit();
		target.RestoreFromJson( json, ctx );

		IActionFactory restored = target.AllActions.Single();
		restored.ShouldBeOfType<FastSlowAction>();
		var fastSlow = (FastSlowAction)restored;
		fastSlow.Cmd.ShouldBeOfType<PlayCardForCost>();
		restored.CouldActivateDuring( Phase.Fast, target ).ShouldBeTrue();
		restored.CouldActivateDuring( Phase.Slow, target ).ShouldBeTrue();
		restored.CouldActivateDuring( Phase.Growth, target ).ShouldBeFalse();

		// present/tax are private primary-constructor fields on PlayCardForCost with no public accessor -
		// ISerializableSelfCmd.ToJson() is the intended, stable window into them (unlike compiler-generated
		// backing field names, which aren't part of the contract), so assert through that instead of reflection.
		JsonArray restoredCmdJson = ( (ISerializableSelfCmd)fastSlow.Cmd ).ToJson( ctx );
		restoredCmdJson.ToJsonString().ShouldBe( ( (ISerializableSelfCmd)new PlayCardForCost( Present.Done, tax: 1 ) ).ToJson( ctx ).ToJsonString() );
	}

	[Fact]
	public void RoundTrips_AvailableAction_RepeatCardForCost_ExcludeSurvives() {
		var gs = new SoloGameState();
		var ctx = CtxFor( gs );

		var original = new TestSpirit();
		original.AddActionFactory( new RepeatCardForCost( "Some Excluded Card" ) );

		JsonObject json = original.ToJson( ctx );

		var target = new TestSpirit();
		target.RestoreFromJson( json, ctx );

		IActionFactory restored = target.AllActions.Single();
		restored.ShouldBeOfType<RepeatCardForCost>();
		restored.Title.ShouldBe( "Repeat Card for Cost" );
		restored.CouldActivateDuring( Phase.Fast, target ).ShouldBeTrue();
		restored.CouldActivateDuring( Phase.Growth, target ).ShouldBeFalse();

		JsonArray restoredJson = ( (ISerializableActionFactory)restored ).ToJson( ctx );
		restoredJson.ToJsonString().ShouldBe( ( (ISerializableActionFactory)new RepeatCardForCost( "Some Excluded Card" ) ).ToJson( ctx ).ToJsonString() );
	}

	[Fact]
	public void RoundTrips_AvailableAction_RepeatCheapestCardForCost() {
		var gs = new SoloGameState();
		var ctx = CtxFor( gs );

		var original = new TestSpirit();
		original.AddActionFactory( new RepeatCheapestCardForCost() );

		JsonObject json = original.ToJson( ctx );

		var target = new TestSpirit();
		target.RestoreFromJson( json, ctx );

		target.AllActions.Single().ShouldBeOfType<RepeatCheapestCardForCost>();
	}

	[Fact]
	public void RoundTrips_AvailableAction_RepeatSpecificCardForCost_ResolvesWrappedCard() {
		var gs = new SoloGameState();
		var ctx = CtxFor( gs );

		var original = new TestSpirit();
		PowerCard card = original.Hand.Single();
		original.AddActionFactory( new SpiritIsland.JaggedEarth.RepeatSpecificCardForCost( card ) );

		JsonObject json = original.ToJson( ctx );

		var target = new TestSpirit();
		target.RestoreFromJson( json, ctx );

		IActionFactory restored = target.AllActions.Single();
		restored.ShouldBeOfType<SpiritIsland.JaggedEarth.RepeatSpecificCardForCost>();

		JsonArray restoredJson = ( (ISerializableActionFactory)restored ).ToJson( ctx );
		restoredJson.ToJsonString().ShouldBe( ( (ISerializableActionFactory)new SpiritIsland.JaggedEarth.RepeatSpecificCardForCost( card ) ).ToJson( ctx ).ToJsonString() );
	}

	[Fact]
	public void RoundTrips_AvailableAction_RepeatCardForFree() {
		var gs = new SoloGameState();
		var ctx = CtxFor( gs );

		var original = new TestSpirit();
		original.AddActionFactory( new RepeatCardForFree( 3 ) );

		JsonObject json = original.ToJson( ctx );

		var target = new TestSpirit();
		target.RestoreFromJson( json, ctx );

		IActionFactory restored = target.AllActions.Single();
		restored.ShouldBeOfType<RepeatCardForFree>();
		restored.Title.ShouldBe( "Replay Card (max cost:3)" );
	}

	[Fact]
	public void RoundTrips_AvailableAction_RelentlessRepeater_ResolvesCardAndSpaceAndCost() {
		var gs = new SoloGameState();
		var ctx = CtxFor( gs );

		var original = new TestSpirit();
		PowerCard card = original.Hand.Single();
		SpaceSpec spec = gs.Board[5];
		Space space = gs.Tokens[spec];
		var relentlessRepeater = new SpiritIsland.NatureIncarnate.RelentlessRepeater( card, space );
		original.AddActionFactory( relentlessRepeater );

		JsonObject json = original.ToJson( ctx );

		var target = new TestSpirit();
		target.RestoreFromJson( json, ctx );

		IActionFactory restored = target.AllActions.Single();
		restored.ShouldBeOfType<SpiritIsland.NatureIncarnate.RelentlessRepeater>();
		// Title bakes in the current cost (card.Cost+1, incrementing per activation) - proves _cost
		// round-tripped, not just card/space identity.
		restored.Title.ShouldBe( $"Repeat {card.Title} on {spec.Label} for {card.Cost + 1} energy." );
	}

	[Fact]
	public void ActionFactoryRegistry_ThrowsForUnregisteredTag() {
		var ctx = CtxFor( new SoloGameState() );
		Should.Throw<NotSupportedException>( () => ActionFactoryRegistry.Deserialize( new JsonArray( "NoSuchActionFactory" ), ctx ) );
	}

	[Fact]
	public void RoundTrips_AvailableAction_ResolveSlowDuringFast() {
		var gs = new SoloGameState();
		var ctx = CtxFor( gs );

		var original = new TestSpirit();
		original.AddActionFactory( new ResolveSlowDuringFast() );

		JsonObject json = original.ToJson( ctx );

		var target = new TestSpirit();
		target.RestoreFromJson( json, ctx );

		target.AllActions.Single().ShouldBeOfType<ResolveSlowDuringFast>();
	}

	[Fact]
	public void RoundTrips_AvailableAction_ResolveSlowDuringFast_OrViseVersa() {
		var gs = new SoloGameState();
		var ctx = CtxFor( gs );

		var original = new TestSpirit();
		original.AddActionFactory( new ResolveSlowDuringFast_OrViseVersa() );

		JsonObject json = original.ToJson( ctx );

		var target = new TestSpirit();
		target.RestoreFromJson( json, ctx );

		target.AllActions.Single().ShouldBeOfType<ResolveSlowDuringFast_OrViseVersa>();
	}

	[Fact]
	public void RoundTrips_AvailableAction_AdHocGrowthAction_NotInGrowthTrack() {
		// SharpFangs.SetupAction and Ocean/Volcano/FinderOfPathsUnseen's setup actions are all
		// GrowthActions built with .ToGrowth() and added directly via AddActionFactory during spirit
		// setup - never part of GrowthTrack.PickGroups, so the position-based "Growth" resolution can't
		// find them. Falls back to "GrowthCmd", resolving the wrapped Cmd via SelfCmdRegistry.
		var gs = new SoloGameState();
		var ctx = CtxFor( gs );

		var original = new TestSpirit();
		var growthAction = new AddBagPresenceToBeastLand().ToGrowth();
		original.AddActionFactory( growthAction );

		JsonObject json = original.ToJson( ctx );

		var target = new TestSpirit();
		target.RestoreFromJson( json, ctx );

		IActionFactory restored = target.AllActions.Single();
		restored.ShouldBeOfType<GrowthAction>();
		var restoredGrowth = (GrowthAction)restored;
		restoredGrowth.Cmd.ShouldBeOfType<AddBagPresenceToBeastLand>();
		restoredGrowth.Phase.ShouldBe( growthAction.Phase ); // Phase.Init, from ToGrowth()
	}

	[Fact]
	public void SelfCmdRegistry_ThrowsForUnregisteredIActOnSpirit() {
		var ctx = CtxFor( new SoloGameState() );
		Should.Throw<NotSupportedException>( () => SelfCmdRegistry.Serialize( SpiritAction.NoAction, ctx ) );
	}

	[Fact]
	public void RoundTrips_UsedInnatePower_RealSpirit() {
		var gs = new SoloGameState();
		var ctx = CtxFor( gs );

		var provider = new SpiritIsland.Basegame.GameComponentProvider();
		Spirit original = provider.MakeSpirit( SpiritIsland.Basegame.Shadows.Name )!;
		InnatePower innate = original.InnatePowers.First();
		original.MarkAsResolved( innate );

		JsonObject json = original.ToJson( ctx );

		Spirit target = provider.MakeSpirit( SpiritIsland.Basegame.Shadows.Name )!;
		target.RestoreFromJson( json, ctx );

		target.InnateWasUsed( target.InnatePowers.First( ip => ip.Title == innate.Title ) ).ShouldBeTrue();
	}

	[Fact]
	public void TargetingSourceStrategyRegistry_ThrowsForUnknownTag() {
		Should.Throw<KeyNotFoundException>( () => TargetingSourceStrategyRegistry.Deserialize( new JsonArray( "NoSuchStrategy" ), CtxFor( new SoloGameState() ) ) );
	}

	[Fact]
	public void RangeCalcRegistry_ThrowsForUnknownTag() {
		Should.Throw<KeyNotFoundException>( () => RangeCalcRegistry.Deserialize( new JsonArray( "NoSuchCalc" ), CtxFor( new SoloGameState() ) ) );
	}

	// -- CustomStateToJson/RestoreCustomStateFromJson - the 7 known CustomMementoValue overrides
	// (docs/GameSerialization-Roadmap.md's Spirit-core-state section). Deliberately independent of
	// CustomMementoValue itself - Mementos are slated for removal later.

	[Fact]
	public void RoundTrips_ShiftingMemoryOfAges_PreparedElements() {
		var ctx = CtxFor( new SoloGameState() );

		var original = new SpiritIsland.JaggedEarth.ShiftingMemoryOfAges();
		original.PreparedElementMgr.PreparedElements[Element.Moon] = 3;
		original.PreparedElementMgr.PreparedElements[Element.Fire] = 1;

		JsonObject json = original.ToJson( ctx );

		var target = new SpiritIsland.JaggedEarth.ShiftingMemoryOfAges();
		target.RestoreFromJson( json, ctx );

		target.PreparedElementMgr.PreparedElements[Element.Moon].ShouldBe( 3 );
		target.PreparedElementMgr.PreparedElements[Element.Fire].ShouldBe( 1 );
	}

	[Fact]
	public void RoundTrips_EmberEyedBehemoth_GrowthTrackTruncation() {
		var ctx = CtxFor( new SoloGameState() );

		var original = new SpiritIsland.NatureIncarnate.EmberEyedBehemoth();
		original.GrowthTrack = new( original.GrowthTrack.Groups.Take( 3 ).ToArray() ); // same truncation DoGrowth does
		original.Incarna.Empowered = true;

		JsonObject json = original.ToJson( ctx );

		var target = new SpiritIsland.NatureIncarnate.EmberEyedBehemoth();
		target.GrowthTrack.Groups.Length.ShouldBe( 4 ); // sanity - fresh construction still has all 4
		target.RestoreFromJson( json, ctx );

		target.GrowthTrack.Groups.Length.ShouldBe( 3 );
		target.Incarna.Empowered.ShouldBeTrue();
	}

	[Fact]
	public void RoundTrips_SerpentPresence_AbsorbedPresences() {
		var spiritA = new TestSpirit();
		var absorbed = new TestSpirit();
		var gs = new GameState( [ spiritA, absorbed ], [ Boards.A ], 0 );
		var ctx = CtxFor( gs );

		spiritA.Presence = new SpiritIsland.FeatherAndFlame.SerpentPresence( spiritA );
		( (SpiritIsland.FeatherAndFlame.SerpentPresence)spiritA.Presence ).AbsorbedPresences.Add( absorbed );

		JsonObject json = spiritA.ToJson( ctx );

		var target = new SpiritIsland.FeatherAndFlame.SerpentPresence( spiritA );
		spiritA.Presence = target;
		spiritA.RestoreFromJson( json, ctx );

		target.AbsorbedPresences.ShouldBe( [ absorbed ] );
	}

	[Fact]
	public void RoundTrips_DancesUpEarthquakes_ImpendingCards() {
		var ctx = CtxFor( new SoloGameState() );

		var original = new SpiritIsland.NatureIncarnate.DancesUpEarthquakes();
		PowerCard card = original.Hand.First();
		original.Hand.Remove( card );
		original.Impending.Add( card );
		original.ImpendingEnergy[card.Title] = 2;
		original.ImpendingEnergyPerRound = 1;
		original.BonusImpendingPlays = 1;

		JsonObject json = original.ToJson( ctx );

		var target = new SpiritIsland.NatureIncarnate.DancesUpEarthquakes();
		target.RestoreFromJson( json, ctx );

		target.Impending.Select( c => c.Title ).ShouldBe( [ card.Title ] );
		target.ImpendingEnergy[card.Title].ShouldBe( 2 );
		target.ImpendingEnergyPerRound.ShouldBe( 1 );
		target.BonusImpendingPlays.ShouldBe( 1 );
	}

	/// <summary> GatewayToken already round-trips as an ordinary board token (ISerializableSpaceEntity,
	/// placed on 2 spaces) - this proves the Spirit's own reference resolves to that same restored
	/// instance rather than a disconnected third one. </summary>
	[Fact]
	public void RoundTrips_FinderOfPathsUnseen_GatewayTokenResolvesToExistingInstance() {
		var gs = new SoloGameState( new SpiritIsland.FeatherAndFlame.FinderOfPathsUnseen(), Boards.A );
		gs.Initialize();
		var ctx = CtxFor( gs );
		var original = (SpiritIsland.FeatherAndFlame.FinderOfPathsUnseen)gs.Spirit;

		Space from = gs.Tokens[gs.Board[2]];
		Space to = gs.Tokens[gs.Board[4]];
		original.GatewayToken = new GatewayToken( original.Presence.Token, from, to );

		JsonObject tokensJson = gs.Tokens.ToJson( ctx );
		JsonObject spiritJson = original.ToJson( ctx );

		var restoredGs = new SoloGameState( new SpiritIsland.FeatherAndFlame.FinderOfPathsUnseen(), Boards.A );
		restoredGs.Initialize();
		var restoredCtx = CtxFor( restoredGs );
		Tokens_ForIsland.FromJson( restoredGs.Tokens, tokensJson, restoredCtx );
		var target = (SpiritIsland.FeatherAndFlame.FinderOfPathsUnseen)restoredGs.Spirit;
		target.RestoreFromJson( spiritJson, restoredCtx );

		GatewayToken restoredOnFrom = restoredGs.Tokens[restoredGs.Board[2]].OfType<GatewayToken>().Single();
		GatewayToken restoredOnTo = restoredGs.Tokens[restoredGs.Board[4]].OfType<GatewayToken>().Single();
		restoredOnFrom.ShouldBeSameAs( restoredOnTo ); // both spaces end up sharing the one live instance
		target.GatewayToken.ShouldBeSameAs( restoredOnFrom ); // spirit resolves to that same instance, not a fresh one
	}

	[Fact]
	public void RoundTrips_FracturedDaysSplitTheSky_RandomizerPositionTimeAndDecks() {
		var gs = new SoloGameState( new SpiritIsland.JaggedEarth.FracturedDaysSplitTheSky() );
		gs.InitMinorDeck();
		gs.InitMajorDeck();
		gs.Initialize();
		var ctx = CtxFor( gs );
		var original = (SpiritIsland.JaggedEarth.FracturedDaysSplitTheSky)gs.Spirit;

		original.OneOrTwo(); // advance the randomizer's replay position past its initial 0
		original.OneOrTwo();

		JsonObject json = original.ToJson( ctx );

		var restoredGs = new SoloGameState( new SpiritIsland.JaggedEarth.FracturedDaysSplitTheSky() );
		restoredGs.InitMinorDeck();
		restoredGs.InitMajorDeck();
		restoredGs.Initialize();
		var restoredCtx = CtxFor( restoredGs );
		var target = (SpiritIsland.JaggedEarth.FracturedDaysSplitTheSky)restoredGs.Spirit;
		target.RestoreFromJson( json, restoredCtx );

		// Same seed (same ShuffleNumber=0 for both) + same replay position => identical next roll.
		target.OneOrTwo().ShouldBe( original.OneOrTwo() );
		target.DtnwMinor.Select( c => c.Title ).ShouldBe( original.DtnwMinor.Select( c => c.Title ) );
		target.DtnwMajor.Select( c => c.Title ).ShouldBe( original.DtnwMajor.Select( c => c.Title ) );
		target.Time.ShouldBe( original.Time );
	}

	/// <summary> InnatePowers/SpecialRules/GrowthTrack are simulated directly through public surface
	/// (matching what a real HealingCard.Claim() would produce) rather than via the internal HealingCard
	/// types themselves, which aren't visible outside SpiritIsland.NatureIncarnate. </summary>
	[Fact]
	public void RoundTrips_WoundedWatersBleeding_HealingStateAndClaimedCards() {
		var ctx = CtxFor( new SoloGameState() );

		var original = new SpiritIsland.NatureIncarnate.WoundedWatersBleeding();
		original.HealingMarkers[Element.Water] = 3;
		original.HealingMarkers[Element.Animal] = 1;
		original.AddSpecialRule( new SpecialRule( "Roiling Waters", "test" ) ); // simulates that card being claimed
		original.InnatePowers[1] = InnatePower.For( typeof( SpiritIsland.NatureIncarnate.CallToAFastnessOfRenewal ) ); // simulates Waters Renew claimed

		JsonObject json = original.ToJson( ctx );

		var target = new SpiritIsland.NatureIncarnate.WoundedWatersBleeding();
		target.GrowthTrack.Groups.Length.ShouldBe( 2 ); // sanity - fresh construction starts at 2
		target.RestoreFromJson( json, ctx );

		target.HealingMarkers[Element.Water].ShouldBe( 3 );
		target.HealingMarkers[Element.Animal].ShouldBe( 1 );
		target.SpecialRules.ShouldContain( r => r.Title == "Roiling Waters" );
		target.InnatePowers[1].Title.ShouldBe( SpiritIsland.NatureIncarnate.CallToAFastnessOfRenewal.Name );
		target.GrowthTrack.Groups.Length.ShouldBe( 3 ); // inferred from "any card claimed", not separately serialized
	}

	[Fact]
	public void RoundTrips_WoundedWatersBleeding_StopHealingRemovesSeekingPathRule() {
		var ctx = CtxFor( new SoloGameState() );

		var original = new SpiritIsland.NatureIncarnate.WoundedWatersBleeding();
		original.StopHealing();

		JsonObject json = original.ToJson( ctx );

		var target = new SpiritIsland.NatureIncarnate.WoundedWatersBleeding();
		target.SpecialRules.ShouldContain( r => r.Title == "Seeking a Path Towards Healing" ); // sanity - fresh construction has it
		target.RestoreFromJson( json, ctx );

		target.SpecialRules.ShouldNotContain( r => r.Title == "Seeking a Path Towards Healing" );
	}

}
