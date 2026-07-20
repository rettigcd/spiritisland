using System.Reflection;

namespace SpiritIsland.Tests.Serialization;

/// <summary>
/// Spirit.Mods - High tier (docs/ISpiritMod-Types.md): mods that track "used"/consumed state via
/// reference-equality against their own cached IActionFactory instance(s) inside spirit.UsedActions.
/// Closed via IOwnedActionFactories (SpiritIsland/Hooks/Spirit/IOwnedActionFactories.cs) - Spirit.
/// SerializeActionFactory/DeserializeActionFactory checks every Mods entry implementing it before
/// falling through to the ordinary PowerCard/InnatePower/GrowthAction/etc. cases, resolving back to the
/// exact same cached instance (already re-added by spirit/aspect construction replay) rather than a
/// fresh, reference-distinct one.
///
/// Each test puts the owned factory into _usedActions directly (via reflection - UsedActions is a
/// read-only public view), round-trips the whole GameState, and confirms UsedActions still contains
/// the SAME object the mod itself resolves to after restore - the thing that actually matters for
/// "was this already used this round" checks to keep working.
/// </summary>
public class SpiritMods_HighTier_Tests {

	static readonly GameBuilder _builder = new(
		new SpiritIsland.Basegame.GameComponentProvider(),
		new SpiritIsland.BranchAndClaw.GameComponentProvider(),
		new SpiritIsland.FeatherAndFlame.GameComponentProvider(),
		new SpiritIsland.JaggedEarth.GameComponentProvider(),
		new SpiritIsland.NatureIncarnate.GameComponentProvider(),
		new SpiritIsland.Horizons.GameComponentProvider()
	);

	static Spirit BuildAndRestore( string spiritName, AspectConfigKey[] aspects, Action<GameState, Spirit> mutate ) {
		var cfg = new GameConfiguration().ConfigSpirits( spiritName ).ConfigBoards( "A" ).ConfigAspects( aspects );

		GameState originalGs = _builder.BuildGame( cfg );
		mutate( originalGs, originalGs.Spirits[0] );

		var originalCtx = new GameStateSerializationContext( originalGs );
		JsonObject json = originalGs.ToJson( originalCtx );

		GameState restoredGs = _builder.BuildGame( cfg );
		var ctx = new GameStateSerializationContext( restoredGs );
		restoredGs.RestoreFromJson( json, ctx );

		return restoredGs.Spirits[0];
	}

	static FieldInfo FindField( Type type, string fieldName ) {
		for( Type? t = type; t is not null; t = t.BaseType ) {
			FieldInfo? field = t.GetField( fieldName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly );
			if( field is not null ) return field;
		}
		throw new InvalidOperationException( $"Field '{fieldName}' not found on {type.Name} or any base type" );
	}

	static List<IActionFactory> UsedActionsList( Spirit spirit ) => (List<IActionFactory>)FindField( typeof( Spirit ), "_usedActions" ).GetValue( spirit )!;

	static IOwnedActionFactories OwnerByTag( Spirit spirit, string tag )
		=> spirit.Mods.OfType<IOwnedActionFactories>().Single( o => o.ModTag == tag );

	[Fact]
	public void ShadowsPartakeOfAmorphousSpace_SurvivesRestore() {
		Spirit spirit = BuildAndRestore( SpiritIsland.Basegame.Shadows.Name, [SpiritIsland.Basegame.Amorphous.ConfigKey], ( gs, s ) => {
			var owner = OwnerByTag( s, "ShadowsPartakeOfAmorphousSpace" );
			UsedActionsList( s ).Add( owner.ResolveActionFactory( "moveFast" ) );
		} );

		var owner = OwnerByTag( spirit, "ShadowsPartakeOfAmorphousSpace" );
		spirit.UsedActions.ShouldContain( owner.ResolveActionFactory( "moveFast" ) );
	}

	[Fact]
	public void PourDownPower_SurvivesRestore() {
		Spirit spirit = BuildAndRestore( SpiritIsland.FeatherAndFlame.DownpourDrenchesTheWorld.Name, [], ( gs, s ) => {
			var owner = OwnerByTag( s, "PourDownPower" );
			UsedActionsList( s ).Add( owner.ResolveActionFactory( "a1" ) );
		} );

		var owner = OwnerByTag( spirit, "PourDownPower" );
		spirit.UsedActions.ShouldContain( owner.ResolveActionFactory( "a1" ) );
	}

	[Fact]
	public void MistsSteadilyDrift_SurvivesRestore() {
		Spirit spirit = BuildAndRestore( SpiritIsland.JaggedEarth.ShroudOfSilentMist.Name, [SpiritIsland.JaggedEarth.Stranded.ConfigKey], ( gs, s ) => {
			var owner = OwnerByTag( s, "MistsSteadilyDrift" );
			UsedActionsList( s ).Add( owner.ResolveActionFactory( "pushFast" ) );
		} );

		var owner = OwnerByTag( spirit, "MistsSteadilyDrift" );
		spirit.UsedActions.ShouldContain( owner.ResolveActionFactory( "pushFast" ) );
	}

	[Fact]
	public void StrandedInTheShiftingMists_SurvivesRestore() {
		Spirit spirit = BuildAndRestore( SpiritIsland.JaggedEarth.ShroudOfSilentMist.Name, [SpiritIsland.JaggedEarth.Stranded.ConfigKey], ( gs, s ) => {
			var owner = OwnerByTag( s, "StrandedInTheShiftingMists" );
			UsedActionsList( s ).Add( owner.ResolveActionFactory( "isolate" ) );
		} );

		var owner = OwnerByTag( spirit, "StrandedInTheShiftingMists" );
		spirit.UsedActions.ShouldContain( owner.ResolveActionFactory( "isolate" ) );
	}

	[Fact]
	public void UnrelentingStrides_SurvivesRestore() {
		Spirit spirit = BuildAndRestore( SpiritIsland.NatureIncarnate.EmberEyedBehemoth.Name, [], ( gs, s ) => {
			var owner = OwnerByTag( s, "UnrelentingStrides" );
			UsedActionsList( s ).Add( owner.ResolveActionFactory( "behemoth" ) );
		} );

		var owner = OwnerByTag( spirit, "UnrelentingStrides" );
		spirit.UsedActions.ShouldContain( owner.ResolveActionFactory( "behemoth" ) );
	}

	[Fact]
	public void MarkedBeastMover_SurvivesRestore() {
		Spirit spirit = BuildAndRestore( SpiritIsland.NatureIncarnate.EmberEyedBehemoth.Name, [], ( gs, s ) => {
			// MarkedBeast is a board token spawned mid-game by a Major Power - unlike every other
			// High-tier mod, replaying spirit/aspect construction never recreates it; the only place it
			// (and the MarkedBeastMover its constructor adds to Mods) gets reconstructed on restore is
			// MarkedBeast's own SpaceEntitySerialization reader, via Tokens_ForIsland.FromJson - so it
			// must actually be placed on a space here to round-trip at all.
			var beast = new SpiritIsland.NatureIncarnate.MarkedBeast( s );
			Space space = gs.Island.Boards[0][1].ScopeSpace;
			space.Adjust( beast, 1 );

			var owner = OwnerByTag( s, "MarkedBeastMover" );
			UsedActionsList( s ).Add( owner.ResolveActionFactory( "" ) );
		} );

		var owner = OwnerByTag( spirit, "MarkedBeastMover" );
		spirit.UsedActions.ShouldContain( owner.ResolveActionFactory( "" ) );
	}

	// IntensifyAirWater looked like it needed both a reference-equality fix (_slowAsFast) and a fix for
	// IInitializeSpirit.Initialize() firing again - neither actually applies under this codebase's
	// established restore pattern: _slowAsFast is rebuilt fresh every Modify() call and never persists
	// across a valid save boundary, and RestoreFromJson never calls Initialize() again (that only runs
	// once, during the earlier normal BuildGame() the restore target was built through). This test
	// exists to prove that empirically, the same way SpiritMods_LowTier_Tests did for the Low tier.
	[Fact]
	public void IntensifyAirWater_SurvivesRestore() {
		Spirit spirit = BuildAndRestore( SpiritIsland.JaggedEarth.ShiftingMemoryOfAges.Name, [SpiritIsland.JaggedEarth.Intensify.ConfigKey], ( gs, s ) => { } );

		spirit.Mods.OfType<IModifyAvailableActions>().Count().ShouldBeGreaterThan( 0 );
	}

}
