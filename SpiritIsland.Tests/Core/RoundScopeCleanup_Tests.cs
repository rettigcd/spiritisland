namespace SpiritIsland.Tests.Core;

/// <summary>
/// Every type that used to read/write GameState.RoundScope now manages its own round-state
/// directly instead - these tests prove each one's reset actually fires, not just that nothing
/// crashed. None of these 5 types had any test coverage before this fix.
/// </summary>
public class RoundScopeCleanup_Tests {

	[Fact]
	public void RunSlowCardsAsFastMod_EveryRound_CleanupResetsUsedCount() {
		var gs = new SoloGameState();
		var mod = new TestSlowCardsAsFastMod(gs.Spirit);

		var usedCountField = typeof(RunSlowCardsAsFastMod_EveryRound).GetField("_usedCount", BindingFlags.NonPublic | BindingFlags.Instance)!;
		usedCountField.SetValue(mod, 3);
		((int)usedCountField.GetValue(mod)!).ShouldBe(3);

		((ICleanupSpiritWhenTimePasses)mod).CleanupSpirit(gs.Spirit);

		((int)usedCountField.GetValue(mod)!).ShouldBe(0);
	}

	class TestSlowCardsAsFastMod(Spirit spirit) : RunSlowCardsAsFastMod_EveryRound(spirit) {
		protected override int AllowedCount => 1;
	}

	[Fact]
	public void FrightfulShadowsEludeDestruction_CleanupResetsUsedThisRound() {
		var spirit = new SpiritIsland.Basegame.Shadows();
		SpiritIsland.Basegame.FrightfulShadowsEludeDestruction.InitAspect(spirit);

		var token = (SpiritIsland.Basegame.FrightfulShadowsEludeDestruction)spirit.Presence.Token;
		var usedField = typeof(SpiritIsland.Basegame.FrightfulShadowsEludeDestruction).GetField("UsedThisRound", BindingFlags.NonPublic | BindingFlags.Instance)!;
		usedField.SetValue(token, true);
		((bool)usedField.GetValue(token)!).ShouldBeTrue();

		// Token is deliberately also in spirit.Mods (see InitAspect) solely so this fires each round.
		spirit.Mods.OfType<ICleanupSpiritWhenTimePasses>().ShouldContain(token);
		((ICleanupSpiritWhenTimePasses)token).CleanupSpirit(spirit);

		((bool)usedField.GetValue(token)!).ShouldBeFalse();
	}

	[Fact]
	public void ReachThroughEphemeralDistance_CleanupResetsUsedThisRound() {
		var spirit = new SpiritIsland.Basegame.Shadows();
		// ReachThroughEphemeralDistance is internal - go through the public Aspect wrapper, the same
		// entry point real spirit setup uses.
		new SpiritIsland.Basegame.Reach().ModSpirit(spirit);

		// Internal type - found via the public interface it registered itself under.
		var mod = spirit.Mods.OfType<ICleanupSpiritWhenTimePasses>().Single();
		var usedField = mod.GetType().GetField("_usedThisRound", BindingFlags.NonPublic | BindingFlags.Instance)!;
		usedField.SetValue(mod, true);
		((bool)usedField.GetValue(mod)!).ShouldBeTrue();

		mod.CleanupSpirit(spirit);

		((bool)usedField.GetValue(mod)!).ShouldBeFalse();
	}

	[Fact]
	public void OneTimeDamageBoost_RemoveIslandMod_TakesItOutOfDispatch() {
		// No more UsedThisRound flag at all - "used" is now represented by the mod not existing in
		// island mods anymore, rather than a flag checked inside a method that's still being called.
		// ModsOfType<T> (Keys.Union(_islandMods).OfType<T>()) is what actually sees island mods -
		// plain OfType<T>/the indexer only look at the Space's own local counts. gs.Tokens[spec] is
		// used directly (per its own doc comment: "Test stuff that is outside of an ActionScope, may
		// use this directly") - SpaceSpec.ScopeSpace goes through ActionScope.Current.AccessTokens,
		// which caches a Space snapshot that wouldn't necessarily reflect a mod added afterward.
		var gs = new SoloGameState();
		var spec = gs.Board[2];
		var boost = new SpiritIsland.Horizons.OneTimeDamageBoost(gs.Spirit, 3);

		gs.AddIslandMod(boost);
		gs.Tokens[spec].ModsOfType<SpiritIsland.Horizons.OneTimeDamageBoost>().ShouldContain(boost);

		gs.RemoveIslandMod(boost);
		gs.Tokens[spec].ModsOfType<SpiritIsland.Horizons.OneTimeDamageBoost>().ShouldNotContain(boost);
	}

	[Fact]
	public void RemoveIslandMod_Decrements_DoesNotWipeOutHigherCounts() {
		// If the same instance were ever added twice, RemoveIslandMod should undo exactly one Add,
		// not wipe out both - mirrors AddIslandMod's ++, not a hard reset to 0. There's no public way
		// to read the raw count for an island mod, so this proves it via presence/absence instead:
		// with count starting at 2, a "reset to 0" implementation would vanish after 1 removal call;
		// a correct decrement stays present until the 2nd call.
		var gs = new SoloGameState();
		var spec = gs.Board[2];
		var mod = new SpiritIsland.JaggedEarth.HabsburgMakeTownsDurable();

		gs.AddIslandMod(mod);
		gs.AddIslandMod(mod);

		gs.RemoveIslandMod(mod);
		gs.Tokens[spec].ModsOfType<SpiritIsland.JaggedEarth.HabsburgMakeTownsDurable>().ShouldContain(mod);

		gs.RemoveIslandMod(mod);
		gs.Tokens[spec].ModsOfType<SpiritIsland.JaggedEarth.HabsburgMakeTownsDurable>().ShouldNotContain(mod);
	}

	[Fact]
	public async Task PowerRangeCalc_TemporaryOverride_RollsBackOnTimePasses() {
		// docs/GameSerialization-Roadmap.md section 10: this used to schedule a separate
		// TimePassesAction.Once closure (unserializable); the rollback now lives directly in
		// Spirit.TimePasses, so it fires from the same round-end call every other cleanup here does.
		var gs = new SoloGameState();
		var extended = new RangeExtender( 1, gs.Spirit.PowerRangeCalc );
		gs.Spirit.PowerRangeCalc = extended;

		gs.Spirit.PowerRangeCalc.ShouldBeSameAs( extended );

		await gs.Spirit.TimePasses( gs );

		gs.Spirit.PowerRangeCalc.ShouldBeSameAs( DefaultRangeCalculator.Singleton );
	}

	[Fact]
	public void GatherPowerFromTheCoolAndDark_CleanupResetsUsedThisRound() {
		var spirit = new SpiritIsland.JaggedEarth.ShroudOfSilentMist();

		// Internal type, constructed inside ShroudOfSilentMist's ctor - found via spirit.Mods.
		var mod = spirit.Mods.OfType<ICleanupSpiritWhenTimePasses>().Single();
		var usedField = mod.GetType().GetField("_usedThisRound", BindingFlags.NonPublic | BindingFlags.Instance)!;
		usedField.SetValue(mod, true);
		((bool)usedField.GetValue(mod)!).ShouldBeTrue();

		mod.CleanupSpirit(spirit);

		((bool)usedField.GetValue(mod)!).ShouldBeFalse();
	}

}
