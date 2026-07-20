namespace SpiritIsland.Tests.Serialization;

/// <summary>
/// Spirit.Mods - Low tier (docs/ISpiritMod-Types.md): mods added unconditionally in a spirit's own
/// constructor, or deterministically by an aspect's ModSpirit, carrying zero extra runtime state. These
/// need no new ToJson/RestoreFromJson code at all - GameState.RestoreFromJson's existing precondition
/// (restore onto a GameState built "the same way" as the original, including the same spirit/aspect
/// selection - see GameState.RestoreFromJson's own remarks) already reconstructs them correctly, since
/// Mods is populated at construction/ModSpirit time and Spirit.RestoreFromJson never touches it. These
/// tests exist to prove that claim empirically for every catalogued Low-tier type, not to add new
/// serialization behavior.
///
/// Some of these mod classes are internal/private to their own project (no InternalsVisibleTo is
/// configured anywhere in this solution), so tests that can't name the concrete type check via the
/// public hook interface instead (there's exactly one such mod on the spirit under test, so a count
/// check is an unambiguous proxy for "the right mod came back").
/// </summary>
public class SpiritMods_LowTier_Tests {

	// A local builder (rather than TestGames.GameBuilder) so NatureIncarnate - needed for
	// RelentlessGazeOfTheSun - doesn't get pulled into every other test's shared Fear/Blight card pool.
	static readonly GameBuilder _builder = new(
		new SpiritIsland.Basegame.GameComponentProvider(),
		new SpiritIsland.BranchAndClaw.GameComponentProvider(),
		new SpiritIsland.FeatherAndFlame.GameComponentProvider(),
		new SpiritIsland.JaggedEarth.GameComponentProvider(),
		new SpiritIsland.NatureIncarnate.GameComponentProvider()
	);

	static Spirit BuildAndRestore( string spiritName, params AspectConfigKey[] aspects ) {
		var cfg = new GameConfiguration().ConfigSpirits( spiritName ).ConfigBoards( "A" ).ConfigAspects( aspects );

		GameState originalGs = _builder.BuildGame( cfg );
		var originalCtx = new GameStateSerializationContext( originalGs );
		JsonObject json = originalGs.ToJson( originalCtx );

		GameState restoredGs = _builder.BuildGame( cfg );
		var ctx = new GameStateSerializationContext( restoredGs );
		restoredGs.RestoreFromJson( json, ctx );

		return restoredGs.Spirits[0];
	}

	[Fact]
	public void RiversDomain_SurvivesRestore() {
		Spirit spirit = BuildAndRestore( SpiritIsland.Basegame.RiverSurges.Name );
		spirit.Mods.OfType<SpiritIsland.Basegame.RiversDomain>().Count().ShouldBe( 1 );
	}

	[Fact]
	public void WreakVengeananceForTheLandsCorruption_SurvivesRestore() {
		Spirit spirit = BuildAndRestore( SpiritIsland.JaggedEarth.VengeanceAsABurningPlague.Name );
		spirit.Mods.OfType<SpiritIsland.JaggedEarth.WreakVengeananceForTheLandsCorruption>().Count().ShouldBe( 1 );
	}

	[Fact]
	public void FlyFastAsThought_SurvivesRestore() {
		Spirit spirit = BuildAndRestore( SpiritIsland.JaggedEarth.ManyMindsMoveAsOne.Name );
		spirit.Mods.OfType<SpiritIsland.JaggedEarth.FlyFastAsThought>().Count().ShouldBe( 1 );
	}

	[Fact]
	public void FinderOfPathsUnseen_IsItsOwnMod_SurvivesRestore() {
		Spirit spirit = BuildAndRestore( SpiritIsland.FeatherAndFlame.FinderOfPathsUnseen.Name );
		spirit.Mods.OfType<SpiritIsland.FeatherAndFlame.FinderOfPathsUnseen>().Single().ShouldBe( spirit );
	}

	// RelentlessPunishment (NatureIncarnate.Spirits.Sun.Mods) is internal - checked via its public hook
	// interface. RelentlessGazeOfTheSun adds no other IHandleActivatedActions mod.
	[Fact]
	public void RelentlessPunishment_SurvivesRestore() {
		Spirit spirit = BuildAndRestore( SpiritIsland.NatureIncarnate.RelentlessGazeOfTheSun.Name );
		spirit.Mods.OfType<IHandleActivatedActions>().Count().ShouldBe( 1 );
	}

	// TwoElementsForMajorCards is a private nested class inside the Immense aspect - checked via its
	// public hook interface. LightningsSwiftStrike+Immense adds no other IHandleCardPlayed mod.
	[Fact]
	public void TwoElementsForMajorCards_SurvivesRestore() {
		Spirit spirit = BuildAndRestore(
			SpiritIsland.Basegame.LightningsSwiftStrike.Name,
			SpiritIsland.Basegame.Spirits.Lightning.Aspects.Immense.ConfigKey );
		spirit.Mods.OfType<IHandleCardPlayed>().Count().ShouldBe( 1 );
	}

	// LocusOfTheSerpentsRegard (FeatherAndFlame.Spirits.Serpent.Mods) is internal - checked via its
	// public hook interface. SerpentSlumbering+Locus adds no other IHandleCardPlayed mod. Also exercises
	// Locus.ModSpirit's other AddActionFactory call (a one-off SpiritAction wrapped by .ToGrowth()) -
	// this used to throw until Locus.PlaceIncarnaAndFireEnergy became a named ISerializableSelfCmd.
	[Fact]
	public void LocusOfTheSerpentsRegard_SurvivesRestore() {
		Spirit spirit = BuildAndRestore( SpiritIsland.FeatherAndFlame.SerpentSlumbering.Name, SpiritIsland.FeatherAndFlame.Locus.Key );
		spirit.Mods.OfType<IHandleCardPlayed>().Count().ShouldBe( 1 );
	}

	// The other 2 call sites the same gap affected (see SelfCmdRegistry's doc comment) - both used to
	// throw NotSupportedException during GameState.ToJson before their SpiritAction became a named
	// ISerializableSelfCmd. No Spirit.Mods involved here; this just proves the GrowthAction round-trips.
	// WarriorSpiritsRaidingParty.PlaceIncarna is internal - checked via Cmd.Description (public on
	// BaseCmd<T>) rather than the concrete type.
	[Fact]
	public void Warrior_PlaceIncarnaGrowthAction_SurvivesRestore() {
		Spirit spirit = BuildAndRestore( SpiritIsland.Basegame.Thunderspeaker.Name, SpiritIsland.Basegame.Warrior.ConfigKey );
		spirit.AllActions.OfType<GrowthAction>()
			.Any( g => g.Cmd.Description == "Place Incarna" )
			.ShouldBeTrue();
	}

	[Fact]
	public void Lair_InitLairGrowthAction_SurvivesRestore() {
		Spirit spirit = BuildAndRestore( SpiritIsland.JaggedEarth.LureOfTheDeepWilderness.Name, SpiritIsland.JaggedEarth.Lair.ConfigKey );
		spirit.AllActions.OfType<GrowthAction>().Count().ShouldBeGreaterThan( 0 );
	}

}
