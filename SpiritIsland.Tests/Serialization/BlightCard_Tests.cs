namespace SpiritIsland.Tests.Serialization;

/// <summary>
/// Round-trips for BlightCard.ToJson/FromJson - docs/GameSerialization-Roadmap.md section 7.
/// Blight *count* isn't tested here - it's ordinary tokens on the "BlightCard" FakeSpace, already
/// covered by Tokens_ForIsland_Tests.
/// </summary>
public class BlightCard_Tests {

	[Fact]
	public void RoundTrips_TypeIdentityAndCardFlipped() {
		var gs = new SoloGameState();
		var serCtx = new GameStateSerializationContext(gs);
		var original = new SpiritIsland.JaggedEarth.AllThingsWeaken();
		original.CardFlipped = true;

		JsonArray json = original.ToJson( serCtx );
		json[0]!.GetValue<string>().ShouldBe( "AllThingsWeaken" );
		json[1]!.GetValue<bool>().ShouldBeTrue();

		BlightCard restored = BlightCard.FromJson( json, serCtx );
		restored.ShouldBeOfType<SpiritIsland.JaggedEarth.AllThingsWeaken>();
		restored.CardFlipped.ShouldBeTrue();
	}

	[Fact]
	public void RoundTrips_NullBlightCard() {
		var gs = new SoloGameState();
		var serCtx = new GameStateSerializationContext(gs);
		var original = new NullBlightCard();

		JsonArray json = original.ToJson( serCtx );
		BlightCard restored = BlightCard.FromJson( json, serCtx );

		restored.ShouldBeOfType<NullBlightCard>();
		restored.CardFlipped.ShouldBeFalse();
	}

	[Fact]
	public void RoundTrips_SlowDissolutionOfWill_NowStatelessLikeMostCards() {
		// Split into SlowDissolutionOfWill (the BlightCard, now stateless) + SlowDissolutionOfWillMod
		// (the IRunBeforeInvaderPhase carrying the per-spirit token choice) - the card itself round-trips
		// the same trivial way as any other blight card now. See SlowDissolutionOfWillMod_Tests below
		// for the state that used to live here.
		var gs = new SoloGameState();
		var serCtx = new GameStateSerializationContext(gs);
		var original = new SpiritIsland.NatureIncarnate.SlowDissolutionOfWill();

		JsonArray json = original.ToJson( serCtx );
		json.Count.ShouldBe( 2 ); // [typeName, CardFlipped] only - no extra state anymore

		BlightCard restored = BlightCard.FromJson( json, serCtx );
		restored.ShouldBeOfType<SpiritIsland.NatureIncarnate.SlowDissolutionOfWill>();
	}

	[Fact]
	public void GameState_BlightCards_RoundTrips_PoolOrderAndIdentity() {
		// GameState.BlightCards (plural) is the not-yet-drawn pool StillHealthyBlightCard.Side2Depleted
		// draws from - distinct from the single active GameState.BlightCard tested above. Reuses
		// BlightCardRegistry directly via BlightCard.ToJson/FromJson, no new registration needed.
		var gs = new SoloGameState();
		var serCtx = new GameStateSerializationContext( gs );

		gs.BlightCards = [ new SpiritIsland.JaggedEarth.AllThingsWeaken() { CardFlipped = true }, new NullBlightCard() ];

		JsonArray json = gs.BlightCardsToJson( serCtx );

		var target = new SoloGameState();
		target.RestoreBlightCardsFromJson( json, serCtx );

		target.BlightCards.Select( bc => bc.GetType() ).ShouldBe( [ typeof( SpiritIsland.JaggedEarth.AllThingsWeaken ), typeof( NullBlightCard ) ] );
		target.BlightCards[0].CardFlipped.ShouldBeTrue();
		target.BlightCards[1].CardFlipped.ShouldBeFalse();
	}

	[Fact]
	public void SlowDissolutionOfWillMod_RoundTrips_ViaPreInvaderPhaseActionRegistry() {
		// SlowDissolutionOfWillMod is internal (IRunBeforeInvaderPhase-only, not ISpaceEntity - see the
		// standing IRunBeforeInvaderPhase/ISpaceEntity correction) - reflection is needed to construct
		// and inspect it directly from here, same as other internal cross-assembly types tested this
		// session (ReachThroughEphemeralDistance, GatherPowerFromTheCoolAndDark). Deserializing goes
		// through the public PreInvaderPhaseActionRegistry.Deserialize though, proving the
		// [ModuleInitializer] registration actually resolves by tag, not just that ToJson/FromJson
		// exist as bare methods.
		var gs = new SoloGameState();
		var serCtx = new GameStateSerializationContext(gs);

		var modType = typeof(SpiritIsland.NatureIncarnate.SlowDissolutionOfWill).Assembly
			.GetType("SpiritIsland.NatureIncarnate.SlowDissolutionOfWillMod")!;
		var original = (IRunBeforeInvaderPhase)Activator.CreateInstance( modType, nonPublic: true )!;

		var replacementsField = modType.GetField("_replacements", BindingFlags.NonPublic | BindingFlags.Instance)!;
		((Dictionary<Spirit,IToken>)replacementsField.GetValue(original)!)[gs.Spirit] = Token.Beast;

		var toJson = modType.GetMethod("ToJson", BindingFlags.Public | BindingFlags.Instance)!;
		var json = (JsonArray)toJson.Invoke(original, [serCtx])!;
		json[0]!.GetValue<string>().ShouldBe( "SlowDissolutionOfWillMod" );

		IRunBeforeInvaderPhase restored = PreInvaderPhaseActionRegistry.Deserialize( json, serCtx );
		restored.ShouldBeOfType( modType );

		var restoredReplacements = (Dictionary<Spirit,IToken>)replacementsField.GetValue(restored)!;
		restoredReplacements[gs.Spirit].ShouldBe( Token.Beast );
	}

}
