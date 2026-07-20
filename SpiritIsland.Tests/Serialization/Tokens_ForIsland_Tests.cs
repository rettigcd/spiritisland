namespace SpiritIsland.Tests.Serialization;

/// <summary>
/// Round-trips for Tokens_ForIsland.ToJson/FromJson - the container driver from
/// docs/GameSerialization-Roadmap.md section 1, built on top of the already-complete
/// ISerializableSpaceEntity catalog (docs/ISpaceEntity-Types.md).
/// </summary>
public class Tokens_ForIsland_Tests {

	[Fact]
	public void RoundTrips_HumanTokensOnARealSpace() {
		var gs = new SoloGameState();
		var space = gs.Board[2];
		space.Given_HasTokens("3T@1,2E@1");
		var serCtx = new GameStateSerializationContext(gs);

		JsonObject json = gs.Tokens.ToJson(serCtx);
		var restored = new Tokens_ForIsland();
		Tokens_ForIsland.FromJson(restored, json, serCtx);

		Space restoredSpace = restored[space];
		restoredSpace.Sum(Human.Town).ShouldBe(3);
		restoredSpace.Sum(Human.Explorer).ShouldBe(2);
		// damaged Town (1 damage taken, remaining health 1) round-tripped as a distinct HumanToken value
		restoredSpace.AllHumanTokens().Any(h => h.HumanClass == Human.Town && h.Damage == 1).ShouldBeTrue();
	}

	[Fact]
	public void RoundTrips_ITokenClassTokens() {
		var gs = new SoloGameState();
		var space = gs.Board[2].ScopeSpace;
		space.Blight.Adjust(2);
		space.Beasts.Adjust(1);
		var serCtx = new GameStateSerializationContext(gs);

		JsonObject json = gs.Tokens.ToJson(serCtx);
		var restored = new Tokens_ForIsland();
		Tokens_ForIsland.FromJson(restored, json, serCtx);

		Space restoredSpace = restored[gs.Board[2]];
		restoredSpace[Token.Blight].ShouldBe(2);
		restoredSpace[Token.Beast].ShouldBe(1);
	}

	[Fact]
	public void RoundTrips_IslandMods_ResolvingSameFakeSpaceAsConstructor() {
		// The "Island-Mods" FakeSpace is created fresh inside Tokens_ForIsland's own constructor -
		// this proves a deserialized instance's AddIslandMod-created entries land in the same place
		// a live instance's would, not a disconnected duplicate space keyed by an equal-but-different
		// FakeSpace reference.
		var gs = new SoloGameState();
		var mod = new SpiritIsland.JaggedEarth.HabsburgMakeTownsDurable();
		gs.AddIslandMod(mod);
		var serCtx = new GameStateSerializationContext(gs);

		JsonObject json = gs.Tokens.ToJson(serCtx);
		var restored = new Tokens_ForIsland();
		Tokens_ForIsland.FromJson(restored, json, serCtx);

		var anySpace = restored[gs.Board[2]];
		anySpace.ModsOfType<SpiritIsland.JaggedEarth.HabsburgMakeTownsDurable>().ShouldNotBeEmpty();
	}

	[Fact]
	public void RoundTrips_TokenDefaults() {
		var gs = new SoloGameState();
		gs.Tokens.TokenDefaults[Human.City] = gs.Tokens.TokenDefaults[Human.City].AddHealth(-1);
		var serCtx = new GameStateSerializationContext(gs);

		JsonObject json = gs.Tokens.ToJson(serCtx);
		var restored = new Tokens_ForIsland();
		Tokens_ForIsland.FromJson(restored, json, serCtx);

		restored.GetDefault(Human.City).FullHealth.ShouldBe(gs.Tokens.GetDefault(Human.City).FullHealth);
		restored.GetDefault(Human.City).FullHealth.ShouldBe(2); // default 3, reduced by 1
	}

	[Fact]
	public void RoundTrips_ArbitraryFakeSpace_ByLabelNotByReference() {
		// Mirrors BlightCard's "virtual space" pattern (a FakeSpace nobody registers anywhere) -
		// SpaceSpecOrFakeByLabel falls back to constructing one fresh, relying on SpaceSpec's
		// label-based Equals/GetHashCode to make it interchangeable with the original.
		var gs = new SoloGameState();
		var fakeSpace = new FakeSpace("Test-Virtual-Space");
		gs.Tokens[fakeSpace].Blight.Adjust(3);
		var serCtx = new GameStateSerializationContext(gs);

		JsonObject json = gs.Tokens.ToJson(serCtx);
		var restored = new Tokens_ForIsland();
		Tokens_ForIsland.FromJson(restored, json, serCtx);

		// a brand new FakeSpace instance with the same label resolves to the same data
		restored[new FakeSpace("Test-Virtual-Space")][Token.Blight].ShouldBe(3);
	}

	[Fact]
	public void RoundTrips_DoesExistsFlag() {
		// Real-space SpaceSpecs are shared, stable objects (SpaceSpecOrFakeByLabel resolves to the
		// game's own instance for a real board space, same as Island.MyMemento's restore already does
		// for NativeTerrain) - so FromJson correctly mutates it in place, matching the rest of the
		// Memento-restore philosophy this driver follows. Proven here by flipping the flag back and
		// forth around the round trip rather than just reading whatever's already there.
		var gs = new SoloGameState();
		var space = gs.Board[2];
		space.ScopeSpace.Adjust(Token.Blight, 1); // ensure the space has an entry in _tokenCounts
		space.DoesExists = false;
		var serCtx = new GameStateSerializationContext(gs);

		JsonObject json = gs.Tokens.ToJson(serCtx);

		space.DoesExists = true; // simulate state having drifted since the snapshot was taken
		Tokens_ForIsland.FromJson(new Tokens_ForIsland(), json, serCtx);

		space.DoesExists.ShouldBeFalse(); // restored from the snapshot, not left at the drifted value
	}

	[Fact]
	public void FromJson_WipesTarget_RatherThanMergingOntoExistingState() {
		// docs/GameSerialization-Roadmap.md section 9: an Adversary's Init/Adjust could mutate a
		// normally-constructed Tokens_ForIsland (island mods, token counts) before this runs. FromJson
		// must discard that pre-existing state entirely rather than merging it with the JSON - otherwise
		// restoring a game with 0 Blight on a space would leave whatever Blight Init happened to add.
		var gs = new SoloGameState();
		var serCtx = new GameStateSerializationContext(gs);
		JsonObject json = gs.Tokens.ToJson(serCtx); // empty snapshot - no tokens anywhere

		var target = new Tokens_ForIsland();
		target[gs.Board[2]].Init(Token.Blight, 5); // simulate Adversary Init mutating it first
		target.AddIslandMod(new SpiritIsland.JaggedEarth.HabsburgMakeTownsDurable());

		Tokens_ForIsland.FromJson(target, json, serCtx);

		target[gs.Board[2]][Token.Blight].ShouldBe(0); // wiped, not left at 5
		target[gs.Board[2]].ModsOfType<SpiritIsland.JaggedEarth.HabsburgMakeTownsDurable>().ShouldBeEmpty();
	}

}
