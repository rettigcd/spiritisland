namespace SpiritIsland.Tests.Serialization;

/// <summary>
/// Proves the DTO+registry serialization design end-to-end against 3 representative
/// Medium-complexity types: a bare `Spirit` reference, a `TargetSpaceCtx` reference plus
/// mutable state, and an array of `ITokenClass` singletons resolved via the token-class
/// registry. See docs/ISpaceEntity-Types.md for the complexity catalog this is based on.
/// </summary>
public class SpaceEntitySerialization_Tests {

	[Fact]
	public async Task SkipBuild_RoundTrips_ArrayOfTokenClasses() {
		var gs = new SoloGameState();
		var space = gs.Board[2];
		var serCtx = new GameStateSerializationContext(gs);

		var original = new SkipBuild("Test Skip", UsageDuration.SkipOneThisTurn, Human.Town, Human.City);

		JsonArray json = SpaceEntitySerialization.Serialize(original, serCtx);
		json[0]!.GetValue<string>().ShouldBe("SkipBuild");
		json[1]!.GetValue<string>().ShouldBe("Test Skip");
		json[3]!.AsArray().Select(n => n!.GetValue<string>()).ShouldBe(["Town", "City"]);

		var restored = (SkipBuild)SpaceEntitySerialization.Deserialize(json, serCtx);
		restored.Text.ShouldBe("Test Skip");

		// prove _stoppedClasses came back as real ITokenClass singletons (via the registry),
		// not just labels, by exercising the actual Skip() behavior
		await using ActionScope scope = await ActionScope.StartSpiritAction(ActionCategory.Spirit_Power, gs.Spirit);
		BuildEngine.InvaderToAdd.Value = Human.Town;

		bool stopped = await restored.Skip(space.ScopeSpace);
		stopped.ShouldBeTrue();
	}

	[Fact]
	public async Task FearOnTownCityDestroy_RoundTrips_PreservesRemainingBudget() {
		var gs = new SoloGameState();
		var space = gs.Board[2];
		space.Given_HasTokens("5T@2");
		var serCtx = new GameStateSerializationContext(gs);

		MistsOfOblivion.FearOnTownCityDestroy original;
		await using(var scope1 = await ActionScope.StartSpiritAction(ActionCategory.Spirit_Power, gs.Spirit)) {
			var ctx = gs.Spirit.Target(space);
			original = new MistsOfOblivion.FearOnTownCityDestroy(ctx);
			space.ScopeSpace.Adjust(original, 1);

			// use up 1 of the 4 allotted (remaining budget: 4 -> 3)
			await ctx.Invaders.DestroyNOfClass(1, Human.Town).ShouldComplete("first-destroy");
		}
		// scope1 disposed - original's own "remove at end of action" cleanup already fired

		// Fear.EarnedFear wraps around at a card-activation threshold, so combine with ActivatedCards
		// (same accounting used in MistsOfOblivion_Tests) to get the true cumulative total.
		int TotalFear() => gs.Fear.ActivatedCards.Count * 4 + gs.Fear.EarnedFear;

		int fearAfterFirst = TotalFear();

		JsonArray json;
		await using(var scope2 = await ActionScope.StartSpiritAction(ActionCategory.Spirit_Power, gs.Spirit))
			json = SpaceEntitySerialization.Serialize(original, serCtx);

		json[0]!.GetValue<string>().ShouldBe("FearOnTownCityDestroy");
		json[3]!.GetValue<int>().ShouldBe(3); // remaining budget captured, not the ctor default of 4

		await using(var scope3 = await ActionScope.StartSpiritAction(ActionCategory.Spirit_Power, gs.Spirit)) {
			var restored = (MistsOfOblivion.FearOnTownCityDestroy)SpaceEntitySerialization.Deserialize(json, serCtx);
			space.ScopeSpace.Adjust(restored, 1);

			// destroy the remaining 4 towns; only 3 more "card fear" should be grantable
			var ctx3 = gs.Spirit.Target(space);
			await ctx3.Invaders.DestroyNOfClass(4, Human.Town).ShouldComplete("remaining-destroys");
		}

		// 4 base fear (1/town, independent of the card) + 3 card fear (the restored, reduced budget - not a fresh 4)
		int fearGrantedInSecondBatch = TotalFear() - fearAfterFirst;
		fearGrantedInSecondBatch.ShouldBe(4 + 3);
	}

	[Fact]
	public async Task TerrorStalksTheLand_RoundTrips_ResolvesCorrectSpiritAmongMultiple() {
		var board = Boards.A;
		var spiritA = new TestSpirit();
		var spiritB = new TestSpirit();
		var gs = new GameState([spiritA, spiritB], [board], 0);
		var serCtx = new GameStateSerializationContext(gs);

		var original = new TerrorStalksTheLand(spiritA);

		JsonArray json = SpaceEntitySerialization.Serialize(original, serCtx);
		json[0]!.GetValue<string>().ShouldBe("TerrorStalksTheLand");
		json[1]!.GetValue<int>().ShouldBe(0); // spiritA is Spirits[0]

		var restored = (TerrorStalksTheLand)SpaceEntitySerialization.Deserialize(json, serCtx);

		// EndlessDark is a global singleton, not scoped to this game - compare deltas rather
		// than absolute contents so this test is robust to whatever else has landed there.
		int AbductedCount() => EndlessDark.Space.ScopeSpace.HumanOfTag(TokenCategory.Invader).Length;
		int abductedBefore = AbductedCount();

		// Case 1: current action belongs to the OTHER spirit - no abduction (proves restored
		// didn't just latch onto "whichever spirit is currently acting")
		SpaceSpec space1 = board[2];
		space1.ScopeSpace.Given_HasTokens("1E@1");
		space1.ScopeSpace.Adjust(restored, 1);
		await using(var scope1 = await ActionScope.StartSpiritAction(ActionCategory.Spirit_Power, spiritB))
			await spiritB.Target(space1).Invaders.DestroyNOfClass(1, Human.Explorer).ShouldComplete("wrong-spirit");
		space1.ScopeSpace.Assert_HasInvaders(""); // destroyed for real, not abducted
		AbductedCount().ShouldBe(abductedBefore);

		// Case 2: current action belongs to spiritA - abduction occurs (proves restored's
		// captured spirit correctly resolved back to spiritA specifically)
		SpaceSpec space2 = board[3];
		space2.ScopeSpace.Given_HasTokens("1E@1");
		space2.ScopeSpace.Adjust(restored, 1);
		await using(var scope2 = await ActionScope.StartSpiritAction(ActionCategory.Spirit_Power, spiritA))
			await spiritA.Target(space2).Invaders.DestroyNOfClass(1, Human.Explorer).ShouldComplete("right-spirit");
		space2.ScopeSpace.Assert_HasInvaders(""); // gone from the land...
		AbductedCount().ShouldBe(abductedBefore + 1); //  ...because it was abducted, not destroyed
	}

	// The 3 tests below spot-check the "Spirit-only cluster" batch conversion (22 types sharing
	// the Spirit-only or Spirit+extra-field shape). Most just resolve a Spirit by index, already
	// proven above; these cover the two genuinely new patterns that batch introduced.

	[Fact]
	public void ChokeTheLandWithGreen_RoundTrips_ViaBaseClassSharedTag() {
		var spirit = new SpiritIsland.Basegame.ASpreadOfRampantGreen();
		var gs = new SoloGameState(spirit);
		var serCtx = new GameStateSerializationContext(gs);

		// The spirit's own SteadyRegeneration presence constructs this as its actual Presence.Token -
		// use that live instance rather than a standalone one, since restore now always resolves back
		// to it (see below), not to a fresh reconstruction.
		var original = (SpiritIsland.Basegame.ChokeTheLandWithGreen)spirit.Presence.Token;

		// Every SpiritPresenceToken subclass with no extra state shares the base's fixed tag now
		// (not GetType().Name) - which concrete subclass this is gets decided by "which Spirit +
		// which aspect," already reproduced by the time a Spirit is reconstructed, so identity here
		// only needs to say which spirit, resolving to that spirit's own live Presence.Token.
		JsonArray json = SpaceEntitySerialization.Serialize(original, serCtx);
		json[0]!.GetValue<string>().ShouldBe("SpiritPresenceToken");
		json[1]!.GetValue<int>().ShouldBe(0);

		var restored = (SpiritIsland.Basegame.ChokeTheLandWithGreen)SpaceEntitySerialization.Deserialize(json, serCtx);
		restored.ShouldBeSameAs(original);
	}

	[Fact]
	public void KeeperToken_RoundTrips_ViaBaseClassSharedTag() {
		// Previously a real gap: KeeperToken had no ModuleInitializer of its own, so a saved game with
		// Keeper of the Forbidden Wilds in play would throw KeyNotFoundException on restore (silently
		// serializing fine, only failing at load time). Now covered for free by the shared base tag.
		var spirit = new SpiritIsland.BranchAndClaw.Keeper();
		var gs = new SoloGameState(spirit);
		var serCtx = new GameStateSerializationContext(gs);

		var original = spirit.Presence.Token;

		JsonArray json = SpaceEntitySerialization.Serialize(original, serCtx);
		var restored = SpaceEntitySerialization.Deserialize(json, serCtx);

		restored.ShouldBeSameAs(original);
		restored.ShouldBeOfType<SpiritIsland.BranchAndClaw.KeeperToken>();
	}

	[Fact]
	public void BlisteringHeat_RoundTrips_DowngradedTokensSurviveViaOwnOverride() {
		// BlisteringHeat is the one SpiritPresenceToken subclass in this previously-broken batch that
		// really does hold extra state (_downgradedTokens) beyond Self, so it needs its own ToJson
		// override/registration rather than relying on the shared base tag alone.
		var spirit = new SpiritIsland.Horizons.RisingHeatOfStoneAndSand();
		var gs = new SoloGameState(spirit);
		var serCtx = new GameStateSerializationContext(gs);
		var space = gs.Board[2];

		var original = spirit.Presence.Token;
		var downgraded = new HumanToken(Human.Explorer).AddDamage(0); // stand-in downgraded token
		var field = original.GetType().GetField("_downgradedTokens", BindingFlags.NonPublic | BindingFlags.Instance)!;
		((HashSet<HumanToken>)field.GetValue(original)!).Add(downgraded);

		JsonArray json = SpaceEntitySerialization.Serialize(original, serCtx);
		json[0]!.GetValue<string>().ShouldBe("BlisteringHeat"); // real extra state -> its own distinct tag

		var restored = SpaceEntitySerialization.Deserialize(json, serCtx);

		restored.ShouldBeSameAs(original);
		((HashSet<HumanToken>)field.GetValue(restored)!).ShouldContain(downgraded);
	}

	[Fact]
	public void ToweringRootsIncarna_RoundTrips_PreservesEmpowered() {
		var spirit = new TestSpirit();
		var gs = new SoloGameState(spirit);
		var serCtx = new GameStateSerializationContext(gs);

		var original = new SpiritIsland.NatureIncarnate.ToweringRootsIncarna(spirit) { Empowered = true };

		JsonArray json = SpaceEntitySerialization.Serialize(original, serCtx);
		json[2]!.GetValue<bool>().ShouldBeTrue(); // extra field beyond the spirit index

		var restored = (SpiritIsland.NatureIncarnate.ToweringRootsIncarna)SpaceEntitySerialization.Deserialize(json, serCtx);
		restored.Empowered.ShouldBeTrue();
		restored.Self.ShouldBeSameAs(spirit);
	}

	[Fact]
	public void MistPusher_RoundTrips_PrivateNestedClassNowRegisters() {
		var spirit = new TestSpirit();
		var gs = new SoloGameState(spirit);
		var serCtx = new GameStateSerializationContext(gs);

		var original = new SpiritIsland.BranchAndClaw.ConfoundingMists.MistPusher(spirit);

		JsonArray json = SpaceEntitySerialization.Serialize(original, serCtx);
		json[0]!.GetValue<string>().ShouldBe("MistPusher");

		var restored = SpaceEntitySerialization.Deserialize(json, serCtx);
		restored.ShouldBeOfType<SpiritIsland.BranchAndClaw.ConfoundingMists.MistPusher>();
	}

	// Spot-checks for the TargetSpaceCtx cluster: the mutable one-shot-flag pattern, and the
	// List<SpaceSpec>-that-grows-after-construction pattern.

	[Fact]
	public async Task Add1FearForFirstDestroyedInvader_RoundTrips_PreservesOneShotFlag() {
		var gs = new SoloGameState();
		var space = gs.Board[2];
		space.Given_HasTokens("2E@1");
		var serCtx = new GameStateSerializationContext(gs);

		SpiritIsland.BranchAndClaw.PortentsOfDisaster.Add1FearForFirstDestroyedInvader original;
		await using(var scope1 = await ActionScope.StartSpiritAction(ActionCategory.Spirit_Power, gs.Spirit)) {
			var ctx = gs.Spirit.Target(space);
			original = new SpiritIsland.BranchAndClaw.PortentsOfDisaster.Add1FearForFirstDestroyedInvader(ctx);
			space.ScopeSpace.Adjust(original, 1);

			// use up the one shot
			await ctx.Invaders.DestroyNOfClass(1, Human.Explorer).ShouldComplete("first-destroy");
		}

		int fearAfterFirst = gs.Fear.ActivatedCards.Count * 4 + gs.Fear.EarnedFear;

		JsonArray json;
		await using(var scope2 = await ActionScope.StartSpiritAction(ActionCategory.Spirit_Power, gs.Spirit))
			json = SpaceEntitySerialization.Serialize(original, serCtx);

		json[3]!.GetValue<bool>().ShouldBeFalse(); // already used, not the ctor default of true

		await using(var scope3 = await ActionScope.StartSpiritAction(ActionCategory.Spirit_Power, gs.Spirit)) {
			var restored = (SpiritIsland.BranchAndClaw.PortentsOfDisaster.Add1FearForFirstDestroyedInvader)SpaceEntitySerialization.Deserialize(json, serCtx);
			space.ScopeSpace.Adjust(restored, 1);

			var ctx3 = gs.Spirit.Target(space);
			await ctx3.Invaders.DestroyNOfClass(1, Human.Explorer).ShouldComplete("second-destroy");
		}

		int fearAfterSecond = gs.Fear.ActivatedCards.Count * 4 + gs.Fear.EarnedFear;
		fearAfterSecond.ShouldBe(fearAfterFirst); // restored kept "already used", granted no more fear
	}

	[Fact]
	public async Task DealVengeanceDamageOnDestroy_RoundTrips_PreservesLandList() {
		var gs = new SoloGameState();
		var targetSpace = gs.Board[2];
		var adjacentSpec = targetSpace.Adjacent.First();
		var serCtx = new GameStateSerializationContext(gs);

		targetSpace.Given_HasTokens("1T@2");
		adjacentSpec.Given_HasTokens("1E@1");

		JsonArray json;
		await using(var scope1 = await ActionScope.StartSpiritAction(ActionCategory.Spirit_Power, gs.Spirit)) {
			var ctx = gs.Spirit.Target(targetSpace);
			var lands = new List<SpaceSpec> { targetSpace, adjacentSpec };
			var original = new VengeanceOfTheDead.DealVengeanceDamageOnDestroy(ctx, lands);
			targetSpace.ScopeSpace.Adjust(original, 1);

			json = SpaceEntitySerialization.Serialize(original, serCtx);

			targetSpace.ScopeSpace.Adjust(original, -1); // remove before the restored copy takes over
		}

		json[3]!.AsArray().Select(n => n!.GetValue<string>()).ShouldBe([targetSpace.Label, adjacentSpec.Label]);

		await using(var scope2 = await ActionScope.StartSpiritAction(ActionCategory.Spirit_Power, gs.Spirit)) {
			var restored = (VengeanceOfTheDead.DealVengeanceDamageOnDestroy)SpaceEntitySerialization.Deserialize(json, serCtx);
			targetSpace.ScopeSpace.Adjust(restored, 1);

			// destroying the town leaves target land empty, so damage can only land in the adjacent
			// land - which is only possible if the restored land list still included it.
			await gs.Spirit.Target(targetSpace).Invaders.DestroyNOfClass(1, Human.Town).AwaitUser(user => {
				user.NextDecision.ChooseFirst();
				user.NextDecision.ChooseFirst();
			}).ShouldComplete("destroy-town");
		}

		adjacentSpec.Assert_HasInvaders("");
	}

	// Spot-checks for the Spirit+string / Spirit+int families.

	[Fact]
	public void OneTimeDamageBoost_RoundTrips_PreservesSpiritAndDamageBoost() {
		var gs = new SoloGameState();
		var serCtx = new GameStateSerializationContext(gs);

		// No "used" flag to capture - a used instance removes itself from island mods (via
		// RemoveIslandMod) rather than tracking a separate UsedThisRound field, so being serialized
		// at all already implies "not yet used".
		var original = new SpiritIsland.Horizons.OneTimeDamageBoost(gs.Spirit, 3);

		JsonArray json = SpaceEntitySerialization.Serialize(original, serCtx);
		json[1]!.GetValue<int>().ShouldBe(serCtx.IndexOf(gs.Spirit));
		json[2]!.GetValue<int>().ShouldBe(3);

		var restored = SpaceEntitySerialization.Deserialize(json, serCtx);
		restored.ShouldBeOfType<SpiritIsland.Horizons.OneTimeDamageBoost>();
	}

	[Fact]
	public void SkipAnyInvaderAction_RoundTrips_DirectlyInstantiatedBaseType() {
		var gs = new SoloGameState();
		var serCtx = new GameStateSerializationContext(gs);

		var original = new SkipAnyInvaderAction("Stop it", gs.Spirit);

		JsonArray json = SpaceEntitySerialization.Serialize(original, serCtx);
		json[0]!.GetValue<string>().ShouldBe("SkipAnyInvaderAction");
		json[2]!.GetValue<string>().ShouldBe("Stop it");

		var restored = (SkipAnyInvaderAction)SpaceEntitySerialization.Deserialize(json, serCtx);
		restored.Text.ShouldBe("Stop it");
	}

	// The polymorphic IToken problem: TriggerAfterNoRavageOrBuild's beastToken is either the
	// Token.Beast singleton or a specific MarkedBeast entity. Both branches of
	// ISerializationContext.SerializeToken/DeserializeToken need proving.

	[Fact]
	public void TriggerAfterNoRavageOrBuild_RoundTrips_WithTokenClassSingleton() {
		var gs = new SoloGameState();
		var space = gs.Board[2];
		var serCtx = new GameStateSerializationContext(gs);

		var ctx = gs.Spirit.Target(space);
		var original = new SpiritIsland.NatureIncarnate.TriggerAfterNoRavageOrBuild(gs.Spirit, ctx, Token.Beast);

		JsonArray json = SpaceEntitySerialization.Serialize(original, serCtx);
		JsonArray tokenJson = json[3]!.AsArray();
		tokenJson[0]!.GetValue<string>().ShouldBe("Class");
		tokenJson[1]!.GetValue<string>().ShouldBe("Beast");

		var restored = (SpiritIsland.NatureIncarnate.TriggerAfterNoRavageOrBuild)SpaceEntitySerialization.Deserialize(json, serCtx);

		// re-serializing the restored instance should produce the exact same JSON
		JsonArray reserialized = SpaceEntitySerialization.Serialize(restored, serCtx);
		reserialized.ToJsonString().ShouldBe(json.ToJsonString());
	}

	[Fact]
	public void TriggerAfterNoRavageOrBuild_RoundTrips_WithMarkedBeastEntity() {
		var gs = new SoloGameState();
		var space = gs.Board[2];
		var serCtx = new GameStateSerializationContext(gs);

		var ctx = gs.Spirit.Target(space);
		var markedBeast = new SpiritIsland.NatureIncarnate.MarkedBeast(gs.Spirit);
		var original = new SpiritIsland.NatureIncarnate.TriggerAfterNoRavageOrBuild(gs.Spirit, ctx, markedBeast);

		JsonArray json = SpaceEntitySerialization.Serialize(original, serCtx);
		JsonArray tokenJson = json[3]!.AsArray();
		tokenJson[0]!.GetValue<string>().ShouldBe("Entity");
		tokenJson[1]!.AsArray()[0]!.GetValue<string>().ShouldBe("MarkedBeast"); // nested ISerializableSpaceEntity

		var restored = (SpiritIsland.NatureIncarnate.TriggerAfterNoRavageOrBuild)SpaceEntitySerialization.Deserialize(json, serCtx);

		// proves DeserializeToken resolved a real, independently-reconstructible MarkedBeast
		JsonArray reserialized = SpaceEntitySerialization.Serialize(restored, serCtx);
		reserialized.ToJsonString().ShouldBe(json.ToJsonString());
	}

	// Spot-checks for the new infrastructure the "remaining ~22" batch needed: HumanToken value
	// serialization, Board-by-name resolution, and SpiritPresence resolution via presence.Token.Self.

	[Fact]
	public void InvadersDontParticipateInRavage_RoundTrips_HumanTokenFidelity() {
		var gs = new SoloGameState();
		var serCtx = new GameStateSerializationContext(gs);

		// a token with non-default health/damage/strife/attack, to prove every field round-trips
		HumanToken damagedTown = new HumanToken(Human.Town).AddDamage(1).AddStrife(2).SetAttack(3);
		var sitOuts = new CountDictionary<HumanToken> { [damagedTown] = 2 };
		var original = new SpiritIsland.JaggedEarth.UnnervingPall.InvadersDontParticipateInRavage(sitOuts);

		JsonArray json = SpaceEntitySerialization.Serialize(original, serCtx);
		var restored = (SpiritIsland.JaggedEarth.UnnervingPall.InvadersDontParticipateInRavage)SpaceEntitySerialization.Deserialize(json, serCtx);

		JsonArray reserialized = SpaceEntitySerialization.Serialize(restored, serCtx);
		reserialized.ToJsonString().ShouldBe(json.ToJsonString());
	}

	[Fact]
	public void InvadersSkip1Board_RoundTrips_BoardByName() {
		var gs = new SoloGameState();
		var serCtx = new GameStateSerializationContext(gs);
		string boardName = gs.Board.Name;

		// force the board selection without a live user prompt
		var original = new SpiritIsland.JaggedEarth.InvadersSkip1Board();
		typeof(SpiritIsland.JaggedEarth.InvadersSkip1Board)
			.GetField("_toSkip", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
			.SetValue(original, gs.Board);

		JsonArray json = SpaceEntitySerialization.Serialize(original, serCtx);
		json[1]!.GetValue<string>().ShouldBe(boardName);

		var restored = (SpiritIsland.JaggedEarth.InvadersSkip1Board)SpaceEntitySerialization.Deserialize(json, serCtx);
		JsonArray reserialized = SpaceEntitySerialization.Serialize(restored, serCtx);
		reserialized.ToJsonString().ShouldBe(json.ToJsonString());
	}

	[Fact]
	public async Task PresenceCountDefend_RoundTrips_ResolvesRealSpiritPresence() {
		var gs = new SoloGameState();
		var space = gs.Board[2];
		var serCtx = new GameStateSerializationContext(gs);

		var original = new PresenceCountDefend(gs.Spirit.Presence, "badge");
		JsonArray json = SpaceEntitySerialization.Serialize(original, serCtx);
		json[2]!.GetValue<string>().ShouldBe("badge");

		var restored = (PresenceCountDefend)SpaceEntitySerialization.Deserialize(json, serCtx);

		// DynamicToken recalculates reactively on IHandleTokenAdded - place restored first (on an
		// empty land, so it has nothing to react to yet), then use AddAsync (not the event-free
		// Adjust) so presence arriving actually fires the notification restored reacts to.
		await using ActionScope scope = await ActionScope.StartSpiritAction(ActionCategory.Spirit_Power, gs.Spirit);
		space.ScopeSpace.Adjust(restored, 1);
		await space.ScopeSpace.AddAsync(gs.Spirit.Presence.Token, 2); // 2 of the spirit's own presence in this land
		space.ScopeSpace.Defend.Count.ShouldBe(2);
	}

	[Fact]
	public void GatewayToken_RoundTrips_ViaSpiritPresenceTokenSelf() {
		var gs = new SoloGameState();
		var space1 = gs.Board[2];
		var space2 = gs.Board[3];
		var serCtx = new GameStateSerializationContext(gs);

		var original = new GatewayToken(gs.Spirit.Presence.Token, space1.ScopeSpace, space2.ScopeSpace);

		JsonArray json = SpaceEntitySerialization.Serialize(original, serCtx);
		json[1]!.GetValue<int>().ShouldBe(0); // spirit index, resolved via presence.Self, not a nested entity
		json[2]!.GetValue<string>().ShouldBe(space1.Label);
		json[3]!.GetValue<string>().ShouldBe(space2.Label);

		var restored = (GatewayToken)SpaceEntitySerialization.Deserialize(json, serCtx);

		// GetLinked proves _from/_to/_presence all resolved to a working GatewayToken
		restored.GetLinked(space1.ScopeSpace)!.SpaceSpec.ShouldBe(space2);
		restored.GetLinked(space2.ScopeSpace)!.SpaceSpec.ShouldBe(space1);
	}

	[Fact]
	public void RavageBehavior_RoundTrips_NestedSequenceStepsAndDamageAdjusters() {
		var gs = new SoloGameState();
		var space = gs.Board[2];
		var serCtx = new GameStateSerializationContext(gs);
		var ctx = gs.Spirit.Target(space);

		var original = new RavageBehavior { AttackersDefend = 3 };
		original.SequenceSteps.Add(new SpiritIsland.BranchAndClaw.InstrumentsOfTheirOwnRuin.DamageInvadersInAdjacentLands(ctx));
		original.DamageAdjusters.Add(new SpiritIsland.JaggedEarth.HabsburgMonarchy.NeighborTownsCauseBonusLandDamage());
		original.DamageAdjusters.Add(new SpiritIsland.NatureIncarnate.DauntedByTheDahan.ReduceAttackBy6Mod());

		JsonArray json = SpaceEntitySerialization.Serialize(original, serCtx);
		json[3]!.GetValue<int>().ShouldBe(3);
		json[1]!.AsArray().Count.ShouldBe(1); // SequenceSteps
		json[2]!.AsArray().Count.ShouldBe(2); // DamageAdjusters

		var restored = (RavageBehavior)SpaceEntitySerialization.Deserialize(json, serCtx);
		restored.AttackersDefend.ShouldBe(3);
		restored.SequenceSteps.Count.ShouldBe(1);
		restored.DamageAdjusters.Count.ShouldBe(2);
		restored.DamageAdjusters[0].ShouldBeOfType<SpiritIsland.JaggedEarth.HabsburgMonarchy.NeighborTownsCauseBonusLandDamage>();
		restored.DamageAdjusters[1].ShouldBeOfType<SpiritIsland.NatureIncarnate.DauntedByTheDahan.ReduceAttackBy6Mod>();

		JsonArray reserialized = SpaceEntitySerialization.Serialize(restored, serCtx);
		reserialized.ToJsonString().ShouldBe(json.ToJsonString());
	}

	// Spot-checks for the "parameterless cluster" batch (31 types): most are pure [Tag]-only, but a
	// few carry real mutable state despite having no constructor params.

	[Fact]
	public void SwedenHeavyMining_RoundTrips_PreservesMiningRushFlag() {
		var gs = new SoloGameState();
		var serCtx = new GameStateSerializationContext(gs);

		var original = new SpiritIsland.Basegame.SwedenHeavyMining() { MiningRush = true };

		JsonArray json = SpaceEntitySerialization.Serialize(original, serCtx);
		json[1]!.GetValue<bool>().ShouldBeTrue();

		var restored = (SpiritIsland.Basegame.SwedenHeavyMining)SpaceEntitySerialization.Deserialize(json, serCtx);
		restored.MiningRush.ShouldBeTrue();
	}

	[Fact]
	public void CountDestroyedTokens_RoundTrips_PreservesCount() {
		var gs = new SoloGameState();
		var serCtx = new GameStateSerializationContext(gs);

		var original = new SpiritIsland.JaggedEarth.TheWoundedWildTurnsOnItsAssailants.CountDestroyedTokens();
		((IHandleTokenRemoved)original).HandleTokenRemovedAsync(null!);
		((IHandleTokenRemoved)original).HandleTokenRemovedAsync(null!);
		original.Count.ShouldBe(2);

		JsonArray json = SpaceEntitySerialization.Serialize(original, serCtx);
		json[1]!.GetValue<int>().ShouldBe(2);

		var restored = (SpiritIsland.JaggedEarth.TheWoundedWildTurnsOnItsAssailants.CountDestroyedTokens)SpaceEntitySerialization.Deserialize(json, serCtx);
		restored.Count.ShouldBe(2);
	}

	[Fact]
	public void HabsburgMakeTownsDurable_RoundTrips_StatelessBaseline() {
		var gs = new SoloGameState();
		var serCtx = new GameStateSerializationContext(gs);

		var original = new SpiritIsland.JaggedEarth.HabsburgMakeTownsDurable();

		JsonArray json = SpaceEntitySerialization.Serialize(original, serCtx);
		json[0]!.GetValue<string>().ShouldBe("HabsburgMakeTownsDurable");

		var restored = SpaceEntitySerialization.Deserialize(json, serCtx);
		restored.ShouldBeOfType<SpiritIsland.JaggedEarth.HabsburgMakeTownsDurable>();
	}

	// Spot-checks for the "simple-primitive" batch (4 types): single primitives / an enum, no
	// domain references to resolve.

	[Fact]
	public void InvaderActionToken_RoundTrips_PreservesLabel() {
		var gs = new SoloGameState();
		var serCtx = new GameStateSerializationContext(gs);

		var original = new InvaderActionToken("Explore");

		JsonArray json = SpaceEntitySerialization.Serialize(original, serCtx);
		json[1]!.GetValue<string>().ShouldBe("Explore");

		var restored = (InvaderActionToken)SpaceEntitySerialization.Deserialize(json, serCtx);
		restored.Label.ShouldBe("Explore");
	}

	[Fact]
	public async Task SkipExploreTo_RoundTrips_PreservesSkipAllFlag() {
		var gs = new SoloGameState();
		var serCtx = new GameStateSerializationContext(gs);

		var original = new SkipExploreTo(skipAll: true);

		JsonArray json = SpaceEntitySerialization.Serialize(original, serCtx);
		json[1]!.GetValue<bool>().ShouldBeTrue();

		var restored = (SkipExploreTo)SpaceEntitySerialization.Deserialize(json, serCtx);

		// prove skipAll came back correctly by exercising Skip() - a false skipAll would
		// remove the token (space.Adjust(this,-1)); true leaves it alone
		var space = gs.Board[2].ScopeSpace;
		space.Init(restored, 1);
		(await restored.Skip(space)).ShouldBeTrue();
		space[restored].ShouldBe(1);
	}

	[Fact]
	public async Task SkipRavage_RoundTrips_PreservesLabelAndDuration() {
		var gs = new SoloGameState();
		var serCtx = new GameStateSerializationContext(gs);

		var original = new SkipRavage("Test Skip Ravage", UsageDuration.SkipAllThisTurn);

		JsonArray json = SpaceEntitySerialization.Serialize(original, serCtx);
		json[1]!.GetValue<string>().ShouldBe("Test Skip Ravage");
		json[2]!.GetValue<int>().ShouldBe((int)UsageDuration.SkipAllThisTurn);

		var restored = (SkipRavage)SpaceEntitySerialization.Deserialize(json, serCtx);
		restored.SourceLabel.ShouldBe("Test Skip Ravage");

		// prove duration came back as SkipAllThisTurn - the token should NOT be removed
		var space = gs.Board[2].ScopeSpace;
		space.Init(restored, 1);
		await restored.Skip(space);
		space[restored].ShouldBe(1);
	}

	[Fact]
	public void SaveDahan_RoundTrips_PreservesMaxPerActionAndRepeat() {
		var gs = new SoloGameState();
		var serCtx = new GameStateSerializationContext(gs);

		// Constructor is private (only reachable via the DestroyFewer factory, which needs a full
		// TargetSpaceCtx to invoke) - use reflection to build/inspect directly, same as testing any
		// other private-constructor DTO round trip.
		var original = (SaveDahan)Activator.CreateInstance(
			typeof(SaveDahan), BindingFlags.NonPublic | BindingFlags.Instance, null, [2, true], null)!;

		JsonArray json = SpaceEntitySerialization.Serialize(original, serCtx);
		json[1]!.GetValue<int>().ShouldBe(2);
		json[2]!.GetValue<bool>().ShouldBeTrue();

		var restored = (SaveDahan)SpaceEntitySerialization.Deserialize(json, serCtx);
		typeof(SaveDahan).GetField("_maxPerAction", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(restored).ShouldBe(2);
		typeof(SaveDahan).GetField("_repeat", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(restored).ShouldBe(true);
	}

	[Fact]
	public void StopDahanDamageAndDestruction_RoundTrips_PreservesSourceName() {
		var gs = new SoloGameState();
		var serCtx = new GameStateSerializationContext(gs);

		var original = new SpiritIsland.Basegame.StopDahanDamageAndDestruction("Infinite Vitality");

		JsonArray json = SpaceEntitySerialization.Serialize(original, serCtx);
		json[1]!.GetValue<string>().ShouldBe("Infinite Vitality");

		var restored = (SpiritIsland.Basegame.StopDahanDamageAndDestruction)SpaceEntitySerialization.Deserialize(json, serCtx);
		restored.ShouldBeOfType<SpiritIsland.Basegame.StopDahanDamageAndDestruction>();
	}

	// The "special core types" batch: TokenClassToken and HumanToken deliberately do NOT implement
	// ISerializableSpaceEntity - they're already handled by other mechanisms (TokenClassByLabel and
	// SerializeHumanToken/DeserializeHumanToken respectively). TokenVariety and Incarna are the 2
	// that actually needed the interface.

	[Fact]
	public void TokenVariety_RoundTrips_ResolvesOriginalByLabel() {
		var gs = new SoloGameState();
		var serCtx = new GameStateSerializationContext(gs);

		var original = new TokenVariety(Token.Beast, "😀");

		JsonArray json = SpaceEntitySerialization.Serialize(original, serCtx);
		json[1]!.GetValue<string>().ShouldBe(Token.Beast.Label);
		json[2]!.GetValue<string>().ShouldBe("😀");

		var restored = (TokenVariety)SpaceEntitySerialization.Deserialize(json, serCtx);
		((IToken)restored).Class.ShouldBe(Token.Beast);
		((IOption)restored).Text.ShouldBe($"{Token.Beast.Label}-😀");
	}

	[Fact]
	public void Incarna_RoundTrips_ResolvesToSameInstanceSpiritOwns() {
		var gs = new SoloGameState();
		var serCtx = new GameStateSerializationContext(gs);

		Incarna original = gs.Spirit.Presence.Incarna;
		original.Empowered = true;

		JsonArray json = SpaceEntitySerialization.Serialize(original, serCtx);
		json[2]!.GetValue<bool>().ShouldBeTrue();

		var restored = (Incarna)SpaceEntitySerialization.Deserialize(json, serCtx);

		// identity-preserving: restored IS the spirit's own Incarna, not a fresh instance
		restored.ShouldBeSameAs(gs.Spirit.Presence.Incarna);
		restored.Empowered.ShouldBeTrue();
	}

	[Fact]
	public void Incarna_SerializeToken_TakesEntityBranchNotClassBranch() {
		// Incarna implements ITokenClass, so without ISerializableSpaceEntity taking priority,
		// SerializeToken would fall into the generic by-label branch - which is broken for Incarna,
		// since TokenClassRegistry only scans Token/Human static holders, not per-spirit instances.
		var gs = new SoloGameState();
		var serCtx = new GameStateSerializationContext(gs);

		Incarna original = gs.Spirit.Presence.Incarna;
		ISerializationContext ctx = serCtx;

		JsonArray json = ctx.SerializeToken(original);
		json[0]!.GetValue<string>().ShouldBe("Entity");

		var restored = ctx.DeserializeToken(json);
		restored.ShouldBeSameAs(gs.Spirit.Presence.Incarna);
	}

	[Fact]
	public async Task FreezePresence_RoundTrips_RestoredTokensAreRecognizedAsFrozen() {
		var gs = new SoloGameState();
		var space = gs.Board[2].ScopeSpace;
		var serCtx = new GameStateSerializationContext(gs);

		var beast = new TokenVariety(Token.Beast, "😀");
		var badland = new TokenVariety(Token.Badlands, "😀");
		var original = new SpiritIsland.JaggedEarth.FreezePresence("Test Freeze", gs.Spirit.Presence, beast, badland);

		JsonArray json = SpaceEntitySerialization.Serialize(original, serCtx);
		json[1]!.GetValue<string>().ShouldBe("Test Freeze");

		var restored = (SpiritIsland.JaggedEarth.FreezePresence)SpaceEntitySerialization.Deserialize(json, serCtx);

		// beast/badland are reference-compared inside IsFrozen(), so prove the round trip actually
		// works by pulling restored's own beast field back out and feeding it through behavior -
		// a fresh unrelated TokenVariety wouldn't be recognized even with the same Class/badge.
		var restoredBeast = (IToken)typeof(SpiritIsland.JaggedEarth.FreezePresence)
			.GetField("<beast>P", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(restored)!;

		var args = new RemovingTokenArgs(space, RemoveReason.MovedFrom) { Token = restoredBeast, Count = 1 };
		await ((IModifyRemovingToken)restored).ModifyRemovingAsync(args);
		args.Count.ShouldBe(0); // blocked - recognized as frozen
	}

	[Fact]
	public void InvadersSitOut_RoundTrips_ResolvesSpirit() {
		// User refactored InvadersSitOut to no longer hold a Quota field at all - it's rebuilt fresh
		// (hardcoded 2xInvader) inside Config() each time, the same "hardcoded, no need to persist"
		// pattern as the SkipBuild_Custom subclasses. That sidesteps the Quota-serialization blocker
		// entirely, leaving just a plain Spirit reference - the simplest shape in the whole catalog.
		var gs = new SoloGameState();
		var serCtx = new GameStateSerializationContext(gs);

		var original = new SpiritIsland.NatureIncarnate.TerrifyingRampage.InvadersSitOut(gs.Spirit);

		JsonArray json = SpaceEntitySerialization.Serialize(original, serCtx);
		json[1]!.GetValue<int>().ShouldBe(serCtx.IndexOf(gs.Spirit));

		var restored = (SpiritIsland.NatureIncarnate.TerrifyingRampage.InvadersSitOut)SpaceEntitySerialization.Deserialize(json, serCtx);
		var restoredSpirit = (Spirit)typeof(SpiritIsland.NatureIncarnate.TerrifyingRampage.InvadersSitOut)
			.GetField("_invaderPicker", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(restored)!;
		restoredSpirit.ShouldBeSameAs(gs.Spirit);
	}

	[Fact]
	public void IntensifyThroughUnderstanding_RoundTrips_ResolvesSpirit() {
		// Split from IntensifyAirWater (Spirit.Mods half, not ISpaceEntity - see class doc comment)
		// so this, the Island Mod half (Moon/Sun/Fire/Plant/Animal/Earth), could become a plain
		// Spirit reference and serialize like any other single-Spirit type in the catalog.
		var spirit = new SpiritIsland.JaggedEarth.ShiftingMemoryOfAges();
		var gs = new SoloGameState(spirit);
		var serCtx = new GameStateSerializationContext(gs);

		var original = new SpiritIsland.JaggedEarth.IntensifyThroughUnderstanding(spirit);

		JsonArray json = SpaceEntitySerialization.Serialize(original, serCtx);
		json[1]!.GetValue<int>().ShouldBe(0);

		var restored = (SpiritIsland.JaggedEarth.IntensifyThroughUnderstanding)SpaceEntitySerialization.Deserialize(json, serCtx);
		var restoredSpirit = (SpiritIsland.JaggedEarth.ShiftingMemoryOfAges)typeof(SpiritIsland.JaggedEarth.IntensifyThroughUnderstanding)
			.GetField("_spirit", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(restored)!;
		restoredSpirit.ShouldBeSameAs(spirit);
	}

	[Fact]
	public void FrightfulShadowsEludeDestruction_RoundTrips_ResolvesToSameInstanceAndPreservesUsedThisRound() {
		// Previously had zero registration at all (a real gap, unrelated to any prior pass) - would
		// have thrown KeyNotFoundException if SpaceEntitySerialization.Deserialize ever hit it.
		var spirit = new SpiritIsland.Basegame.Shadows();
		SpiritIsland.Basegame.FrightfulShadowsEludeDestruction.InitAspect(spirit);
		var gs = new SoloGameState(spirit);
		var serCtx = new GameStateSerializationContext(gs);

		var original = (SpiritIsland.Basegame.FrightfulShadowsEludeDestruction)spirit.Presence.Token;
		typeof(SpiritIsland.Basegame.FrightfulShadowsEludeDestruction).GetField("UsedThisRound", BindingFlags.NonPublic | BindingFlags.Instance)!.SetValue(original, true);

		JsonArray json = SpaceEntitySerialization.Serialize(original, serCtx);
		json[0]!.GetValue<string>().ShouldBe("FrightfulShadowsEludeDestruction");
		json[1]!.GetValue<int>().ShouldBe(serCtx.IndexOf(spirit));
		json[2]!.GetValue<bool>().ShouldBeTrue();

		var restored = (SpiritIsland.Basegame.FrightfulShadowsEludeDestruction)SpaceEntitySerialization.Deserialize(json, serCtx);

		// identity-preserving: restored IS the spirit's own presence token, not a fresh instance
		restored.ShouldBeSameAs(spirit.Presence.Token);
		typeof(SpiritIsland.Basegame.FrightfulShadowsEludeDestruction).GetField("UsedThisRound", BindingFlags.NonPublic | BindingFlags.Instance)!
			.GetValue(restored).ShouldBe(true);
	}

}
